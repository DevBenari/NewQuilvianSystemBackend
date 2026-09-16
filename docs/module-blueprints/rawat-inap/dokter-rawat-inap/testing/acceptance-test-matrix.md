# Acceptance Test Matrix — Sub-modul `dokter-rawat-inap` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `dokter-rawat-inap` |
| Contract version | **`0.6.0`** |
| `last_changed_in` | **`0.6.0`** — bagian 12 s.d. 14 lahir: ruang kerja, kewenangan penulis, registrasi dan penguncian, Catatan Saya, verifikasi CPPT, pesanan dan instruksi, Resep Harian, template, rekonsiliasi, sliding scale, jenis catatan, Resume Medis. Sebelumnya `0.5.0` bagian 3A |
| Status | **`draft`** untuk `0.6.0`. `0.5.0` **`approved`** — disetujui **Muhammad Hamzah** 2026-09-11 lewat `RWI-DEC-105` |
| `input_revision` | `02-backend-architecture.md` `0.4`; seluruh kontrak `0.4.0`; arsitektur domain `0.2` |
| `input_hash` | Arsitektur domain SHA-256 `226c6ef1e4bfec544c366b265fe1e4530e80c510da33c1a9eaf2e62161d0b717` |
| Backend SHA | `93b3227c431401d8f586dec4e1fb25fbf41766e3` |
| `approved_by` / `approved_at` | **Muhammad Hamzah** / **2026-09-09** untuk `0.4.0`; `0.3.0` disetujui 2026-09-03 |
| Tanggal | 2 September 2026; disetujui 3 September 2026; diamendemen 9 September 2026 |

Dari **63** skenario di bawah, **27** adalah jalur gagal. Sembilan skenario — bagian 11 — baru pada `0.4.0`.

> **Keadaan awal yang wajib diketahui.** Bukti `DOK-TRC-VER-01` menyatakan **tidak ditemukan satu
> pun** uji otomatis untuk konsultasi, pengkajian, CPPT, tindakan, resep, radiologi rawat inap,
> maupun ruang kerja dokter. Dua puluh enam uji fondasi yang lulus hanya menyentuh episode,
> penugasan, pendaftaran layanan, dan disiplin laboratorium. Seluruh baris di bawah karena itu
> **belum ada satu pun**, dan itulah sebabnya `ARCH-GAP-016` menahan klaim kesiapan.

---

## 0. Perbaikan yang wajib diuji lebih dulu — `DOK-TRC-DEF-01` ★ baru

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `DOK-TRC-DEF-01` | Membuat catatan dokter untuk pasien **tanpa antrean** | Integration | `201`, **bukan** `500`. Ini yang gagal hari ini |
| `DOK-TRC-DEF-01` | **Gagal:** memastikan tidak ada data antrean yang tersentuh pada jalur tanpa antrean | Integration | Nol perubahan pada baris antrean mana pun |
| `RWI-DEC-051` | Membuat catatan dokter untuk pasien IGD lewat jalur lamanya | **Regression** | `201`; perilaku IGD tidak berubah |
| `RWI-DEC-051` | Membuat catatan dokter poliklinik lewat antrean | **Regression** | `201`; perilaku poliklinik tidak berubah |

> Keempat baris ini **wajib hijau sebelum** cabang episode dinyalakan. Menyalakan cabang episode di
> atas jalur yang gagal berarti mengundang pasien rawat inap ke dalam kegagalan yang sudah
> diketahui.

---

## 1. Konteks klinis rawat inap — `INT-DOK-01`, `INT-DOK-02`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-CAP022-01` | Dokter membuat kajian medis pada perawatan yang berjalan, tanpa antrean dan tanpa kunjungan IGD | Integration | `201`; penanda antrean kosong, penanda episode terisi |
| `AC-CAP020-02` | Catatan dokter dapat dibuat walaupun **pengkajian awal keperawatan belum selesai** | Integration | `201`; tidak ada pemeriksaan silang ke pengkajian keperawatan |
| `AC-CAP023-01` | Resep dibuat dari konteks rawat inap tanpa kunjungan IGD aktif | Integration | `201` |
| `AC-CAP024-03` | Tindakan tidak menuntut konteks khusus IGD | Integration | `201` |
| `VAL-DOK-04` | **Gagal:** catatan dokter poliklinik tanpa antrean | Integration | `400`; **membuktikan perilaku poliklinik tidak berubah** |
| `VAL-DOK-26` | **Gagal:** penanda episode terisi tetapi milik perawatan pasien lain | Integration | `400`; tidak ada baris tersimpan |
| `RWI-RULE-026` aturan 4 | Catatan dokter **kedua** pada satu kunjungan rawat inap diterima | Integration | Dua baris catatan pada satu kunjungan |
| `RWI-AC-143` | **Gagal:** catatan kedua pada kunjungan **rawat jalan** tetap ditolak **dengan pesan yang sama persis** | **Regression** | Kode dan kalimat penolakan identik dengan sebelum perubahan |
| `RWI-RULE-026` aturan 5 | Resep kedua sepanjang perawatan diterima | Integration | Dua resep aktif |
| `RWI-AC-143` | **Gagal:** resep aktif kedua pada kunjungan rawat jalan tetap ditolak | **Regression** | Ditolak seperti sebelumnya |

---

## 2. Kajian medis dan catatan dokter — `CAP-020`, `CAP-022`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-CAP022-02` | Kajian medis dan catatan harian punya record serta lifecycle **berbeda** | Integration | Dua tabel berbeda; mengubah catatan harian tidak menyentuh baris kajian medis |
| PRD `CAP-022` aturan 3 | Catatan harian **tidak menimpa** kajian medis final | Integration | Isi kajian medis sama persis sebelum dan sesudah tiga catatan harian ditulis |
| `AC-CAP020-01` | Dua catatan pada hari berbeda tersimpan sebagai dua baris lini masa | Integration | Dua baris terurut waktu klinis |
| PRD `CAP-020` aturan 2 | Waktu klinis terpisah dari waktu penulisan | Integration | Catatan ditulis pukul 11.00 untuk pemeriksaan pukul 07.40 tampil pada urutan pukul 07.40 |
| `INV-DOK-10` | Koreksi catatan final tersimpan sebagai addendum bernomor urut | Integration | Isi asli **tidak berubah**; alasan koreksi tersimpan |
| `AC-CAP020-03` | **Episode `Closed` menolak catatan baru** | Integration | `422` |
| `AC-CAP020-03` | Episode `Closed` **menerima koreksi** catatan lama, dan tidak mengaktifkan kembali episode | Integration | `200`; status episode tetap `Closed`; tempat tidur tidak berubah |
| `VAL-DOK-12` | **Gagal:** menyelesaikan catatan dengan keempat bagian kosong | Integration | `400` |
| `VAL-DOK-11` | **Gagal:** menyelesaikan kajian medis tanpa diagnosis | Integration | `400` |
| `VAL-DOK-14` | **Gagal:** waktu klinis sebelum pasien masuk kamar | Integration | `400` |
| `VAL-DOK-05` | **Gagal:** perawat mencoba membuat kajian medis | Integration | `403` |

---

## 3. Catatan terpadu dan verifikasi — `CAP-021`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-CAP021-01` | Catatan dokter dan catatan perawat tampil sebagai entry terpisah beserta penulis dan profesinya | Integration | Dua baris, profesi berbeda |
| `AC-CAP021-03` | **Verifikator tidak menggantikan penulis asli** | Integration | Penulis asli tidak berubah; verifikator terisi terpisah |
| `INV-DOK-11` | Verifikasi oleh DPJP yang aktif **pada saat verifikasi**, bukan yang aktif saat catatan ditulis | Integration | Verifikasi oleh DPJP pengganti diterima; oleh DPJP lama ditolak |
| `AC-CAP021-02` | Keterlambatan verifikasi terpantau menurut kebijakan aktif | Integration | Baris muncul pada daftar pantau |
| `VAL-DOK-24` | Kebijakan verifikasi **belum ada**: seluruh catatan tidak diwajibkan, daftar pantau kosong | Integration | Pencatatan tetap `201`; nol baris menunggu |
| `VAL-DOK-25` | Catatan terlambat **tidak menahan** penulisan catatan berikutnya | Integration | Catatan berikutnya tetap `201` |
| `VAL-DOK-07` | **Gagal:** dokter jaga yang bukan DPJP mencoba memverifikasi | Integration | `403`; keadaan verifikasi tidak berubah |
| PRD `CAP-021` aturan 6 | Koreksi catatan terverifikasi mengembalikannya ke menunggu verifikasi | Integration | Keadaan kembali menunggu |

---


## 3A. Penutupan jalur hapus dan penegakan penulis — `0.5.0` ★ baru

Menyerap `RWI-DEC-098` dan `RWI-DEC-099`. Seluruh baris di bawah ini **belum pernah ada** pada
revisi sebelumnya.

### 3A.1 Jalur hapus catatan terpadu

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-DOK-060` | `DELETE /patient-integrated-progress-notes/{id}` dipanggil | Integrasi | **`404`**, bukan `403`. Route tidak ada sama sekali |
| `AC-DOK-061` | Membatalkan catatan berstatus `Final` | Integrasi | `422`, disertai keterangan bahwa koreksi dilakukan lewat addendum |
| `AC-DOK-062` | Membatalkan catatan berstatus `Terverifikasi` | Integrasi | `422` |
| `AC-DOK-063` | Membatalkan catatan berstatus `Draf` beserta alasan | Integrasi | `200`, catatan terbaca sebagai dibatalkan beserta alasan dan pelakunya |
| `AC-DOK-064` | Membatalkan catatan `Draf` tanpa alasan | Integrasi | `400` |
| `AC-DOK-065` | Catatan yang sudah dibatalkan **tetap terbaca** pada rekam medis dan audit | Integrasi | Baris masih ada, tidak hilang dari pembacaan normal |
| `AC-DOK-066` | Regresi Rawat Jalan: jalur pembatalan CPPT rawat jalan tidak ikut berubah perilakunya | Integrasi | Perilaku rawat jalan sama persis seperti sebelum perubahan. **Wajib**, karena controller-nya dipakai bersama |

### 3A.2 Penulis dan kewenangan dokter

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-DOK-067` | Pengguna tanpa `ApplicationUser.DoctorId` menulis catatan | Integrasi | `403` |
| `AC-DOK-068` | Dokter tanpa penugasan aktif pada episode itu menulis | Integrasi | `403` |
| `AC-DOK-069` | Dokter mengirim `DoctorId` milik dokter lain | Integrasi | `403`, dan **nol** baris tersimpan atas nama pihak lain |
| `AC-DOK-070` | Dokter dengan penugasan berakhir menulis dengan waktu klinis **di dalam** periodenya | Integrasi | `200`. Membuktikan penilaian memakai waktu klinis |
| `AC-DOK-071` | Dokter dengan penugasan berakhir menulis dengan waktu klinis **di luar** periodenya | Integrasi | `403`. Membuktikan backdating tidak dapat dipakai melewati periode |
| `AC-DOK-072` | Kelima grup jalur tulis memanggil resolver dengan dokter pelaku | Integrasi | Satu test per grup: Doctor Consultation, Patient Assessment, Patient Integrated Progress Note, Patient Diagnosis, Patient Procedure. **Lima test, bukan satu** |
| `AC-DOK-073` | Verifikasi CPPT oleh dokter tanpa penugasan aktif | Integrasi | `403` |
| `AC-DOK-074` | Verifikasi CPPT **tidak** mengubah penulis aslinya | Integrasi | Penulis asli sama persis sebelum dan sesudah verifikasi |
| `AC-DOK-075` | Seluruh skenario negatif dijalankan memakai peran nyata, bukan SuperAdmin | Integrasi | Bukti peran yang dipakai tercatat pada laporan task |

### 3A.3 Kenapa test hak akses lama tidak cukup

`Gelombang 1A` melahirkan **nol** Resource dan **nol** Action baru pada sub-modul ini. Akibatnya
seluruh test hak akses yang sudah ada **tetap lulus tanpa disentuh**, baik sebelum maupun sesudah
perbaikan.

Itu berarti test hak akses **tidak dapat** dipakai sebagai bukti bahwa penjaga baru bekerja. Satu-
satunya bukti yang sah adalah skenario negatif per-pasien pada bagian 3A.2. Menyatakan `Gelombang
1A` selesai karena test hak akses hijau adalah kesimpulan yang salah.

---
## 4. Event visite — `CAP-025`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-150` | Visite dicatat pukul 07.40; catatan SOAP baru dibuat pukul 07.52 | Integration | Riwayat menampilkan waktu **07.40**, bukan 07.52 |
| `RWI-AC-150` | Visite dicatat pukul 07.40 dan **tidak ada** SOAP sama sekali | Integration | Riwayat tetap menampilkan visite pukul 07.40 |
| `RWI-AC-151` | **Tiga catatan ditulis tanpa satu pun event visite** | Integration | Jumlah visite tetap **nol** |
| `RWI-AC-152` | Dua pengiriman dengan kunci permintaan yang sama | Integration | **Satu** kejadian, identitas sama, kode `200` pada pengiriman kedua |
| `RWI-AC-152` | Dua permintaan **bersamaan** dengan kunci sama | Integration terhadap **PostgreSQL sungguhan** | Satu baris. **Provider InMemory tidak dapat membuktikan unique index** |
| `RWI-AC-153` | Riwayat menampilkan episode, dokter, peran, waktu, pencatat, dan tautan dokumen bila ada | Integration | Seluruh kolom terbaca pada satu pembacaan |
| `RWI-AC-154` | dr. Andi visite pukul 07.40 dan 16.10 pada tanggal yang sama | Integration | **Dua** baris riwayat dan hitungan **dua** |
| `RWI-AC-155` | Kiriman ulang berkunci sama tidak dihitung dua; kejadian baru berkunci berbeda dihitung sebagai visite berikutnya | Integration | Hitungan 1 lalu 2 |
| `RWI-AC-156` | Billing mengagregasikan dua kejadian menjadi satu tagihan harian | Integration | Riwayat klinis **tetap menampilkan dua kejadian** tanpa perubahan waktu, dokter, maupun jejak audit |
| `INV-DOK-08` | Kejadian dibatalkan beralasan, lalu dicatat ulang | Integration | Dua baris; yang batal tetap terlihat beserta alasannya; hitungan **satu** |
| `VAL-DOK-08` | **Gagal:** perawat mencoba mencatat visite | Integration | `403` |
| `VAL-DOK-27` | **Gagal:** mencatat visite tanpa kunci permintaan | Integration | `400` |
| `VAL-DOK-28` | **Gagal:** membatalkan tanpa alasan | Integration | `400`; kejadian tetap berlaku |
| `VAL-DOK-29` | **Gagal:** membatalkan kejadian yang sudah batal | Integration | `409` |
| `VAL-DOK-16` | **Gagal:** waktu visite melewati waktu sekarang | Integration | `400` |
| `VAL-DOK-18` | Visite kedua pada jam berdekatan **diperingatkan, bukan ditolak** | Integration | `200`; dua kejadian tersimpan bila dilanjutkan |

---

## 5. Tindakan, resep, dan penunjang — `CAP-023`, `CAP-024`, `CAP-015`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-CAP024-01` | **Gagal:** tindakan disimpan untuk pasangan pasien dan kunjungan yang tidak cocok | Integration | `400` |
| `AC-CAP024-02` | Percobaan ulang tidak menghasilkan tindakan maupun tagihan ganda | Integration terhadap PostgreSQL | Satu baris tindakan, satu fakta klinis |
| PRD `CAP-024` aturan 5 | Kegagalan Billing **tidak menghilangkan** catatan tindakan | Integration | Catatan tetap selesai; penerbitan fakta menyimpan hasil gagalnya |
| `AC-CAP023-02` | Status pemenuhan resep dapat dibaca kembali dengan pengenal yang sama | Integration | Status Farmasi terbaca |
| `AC-CAP023-03` | **Obat pulang dapat dibedakan** dari resep harian | Integration | Tersaring tersendiri menurut jenis resep |
| `VAL-DOK-21` | **Gagal:** sub-modul ini mencoba menandai obat sudah diserahkan | Integration | `403`; status Farmasi tidak berubah |
| `AC-CAP015-01` | **Gagal:** pesanan laboratorium episode A diproses sebagai milik episode B | Integration | `400` |
| `AC-CAP015-01` | **Gagal:** pesanan **radiologi** episode A diproses sebagai milik episode B | Integration | `400` |
| `AC-CAP015-02` | Hasil laboratorium final terbaca dari ruang kerja **tanpa baris salinan** | Integration | Nol tabel hasil baru; data berasal dari modul pemiliknya |
| `AC-CAP015-02` | Hasil **radiologi** final terbaca tanpa baris salinan | Integration | Sama |
| `VAL-DOK-30` | Hasil yang **belum final** ditampilkan dengan penanda, bukan sebagai hasil sah | Integration | Penanda "belum final" terbaca pada response |
| `VAL-DOK-31` | **Gagal:** hasil milik kunjungan di luar perawatan yang dibuka tidak ikut tampil | Integration | Nol baris milik episode lain |
| `VAL-DOK-23` | **Gagal:** sub-modul ini mencoba menulis hasil penunjang | Integration | `403` |

---

## 6. Hak akses per peran

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `BE-RWI-034` | Seluruh endpoint baru dipanggil peran **non-SuperAdmin** yang berhak | Integration | `200`/`201`, **bukan** `403`. Pelajaran dari sembilan endpoint yang pernah terkunci |
| `permission-audit-matrix.md` bagian 2 | Supervisor klinis dapat membatalkan kejadian visite | Integration | `200` |
| `permission-audit-matrix.md` bagian 2 | **Gagal:** petugas admisi membuka ruang kerja dokter | Integration | `403` |

---

## 7. Penjaga batas sub-modul

Skenario yang tidak diturunkan dari requirement mana pun, melainkan dari `RWI-DEC-081`, batas
kepemilikan, dan aturan penamaan backend.

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-DEC-081` | Nol entity berawalan `Inp` yang menyimpan catatan dokter, catatan terpadu, kajian medis, resep, tindakan, atau visite | Architecture test | Pemindaian konteks database menemukan **nol** entity demikian |
| `RUL-DOK-01`, `RUL-DOK-02` | Nol jalur tulis dari modul Rawat Inap menuju status pemenuhan resep maupun hasil penunjang | Architecture test | Pemindaian endpoint dan service menemukan nol penulisan |
| **`QBE-NAM-001`** ★ | **Nol entity baru berawalan `Trx*`** pada perubahan ini | Architecture test | Entity visite bernama `CliPhysicianVisit`; tabelnya `public."CliPhysicianVisit"` |
| `RWI-RULE-026` aturan 2 | Nol baris antrean dibuat untuk pasien rawat inap | Integration | Jumlah baris antrean sebelum dan sesudah alur dokter **identik** |

> Test pertama **dibagi** dengan `keperawatan/testing/acceptance-test-matrix.md` — satu test yang
> menjaga kedua sub-modul, bukan dua test kembar.

---

## 8. Frontend

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `DOK-TRC-FE-01` | Daftar pasien pada ruang kerja dokter berasal dari census episode | Component/state test | Permintaan yang dikirim menuju census, **bukan** antrean rawat jalan |
| `DOK-TRC-FE-01` | **Gagal:** tidak ada aksi panggil, lewati, maupun tidak hadir di seluruh ruang kerja | Component test | Nol pemanggilan aksi antrean |
| `03-frontend-architecture.md` bagian 3.1 | Kegagalan memuat konteks pasien **menonaktifkan seluruh tombol tulis** | Component test | Tombol tulis nonaktif; pesan beserta tombol coba lagi tampil |
| `03-frontend-architecture.md` bagian 3.1 | Kegagalan memuat riwayat alergi **ditampilkan**, tidak disembunyikan | Component test | Pesan kegagalan alergi terlihat menonjol |

---

## 9. Koreksi dokumen final — `RWI-DEC-086` s.d. `RWI-DEC-088`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-157` | Dokter menekan Selesai pada catatan perkembangan | Integration | Catatan terdaftar pada mesin keutuhan sebagai tertanda tangan, dan percobaan menyunting isinya sesudahnya ditolak |
| `RWI-AC-157` | **Gagal:** pendaftaran ke mesin keutuhan gagal saat finalisasi | Integration | **Finalisasi ikut batal.** Tidak ada catatan final yang tidak dapat dikoreksi |
| `RWI-AC-158` | Dokter menambah koreksi beralasan pada catatannya yang sudah final | Integration | Isi asli terbaca sama persis; koreksi tampil sebagai baris bernomor urut beserta alasan dan waktunya |
| `RWI-AC-159` | **Gagal:** menambah koreksi pada catatan yang belum final | Integration | `400`; pesannya mengarahkan menyunting langsung |
| `RWI-AC-160` | Catatan disunting berkali-kali sebelum diselesaikan | Integration | **Satu** catatan utuh, **nol** koreksi tercatat |
| `RWI-AC-161` | Pada episode yang sudah ditutup, koreksi catatan lama diterima | Integration | `200`; status episode tetap tertutup, tempat tidur tidak berubah, lama dirawat tidak bergeser |
| `RWI-AC-161` | **Gagal:** catatan **baru** pada episode yang sudah ditutup | Integration | `422` |
| `RWI-AC-162` | Kajian medis dan tindakan yang sudah diselesaikan dapat dikoreksi dengan cara yang sama | Integration | Keduanya terdaftar pada mesin keutuhan; koreksinya diterima |
| `RWI-AC-163` | Setelah kepala unit menerbitkan penetapan berhalangan, DPJP aktif episode itu menambah koreksi pada catatan dokter tersebut | Integration | `201` |
| `RWI-AC-164` | Koreksi atas nama dokter lain | Integration | **Penulis asli tetap tercantum sebagai penulis catatan**; dokter pengganti dan penandanya hanya muncul pada baris koreksi |
| `RWI-AC-165` | **Gagal:** penetapan berhalangan tanpa masa berlaku | Integration | `400`; penetapan tidak terbentuk |
| `RWI-AC-166` | Akun dokter penulis nonaktif | Integration | DPJP aktif dapat langsung mengoreksi **tanpa** penetapan apa pun |
| `RWI-AC-167` | **Gagal:** dokter yang bukan DPJP aktif episode itu mengoreksi atas nama penulis lain, walaupun butir hak akses pengganti dimilikinya dan penetapannya berlaku | Integration | `403`. **Ini penjaga kewenangan per pasien, bukan penjaga hak akses** — mesin hak akses akan meloloskannya |

> Baris terakhir adalah yang paling mudah lolos dari perhatian: seluruh pemeriksaan hak akses
> **berhasil**, dan penolakannya justru datang dari aturan bisnis. Test yang hanya menguji hak akses
> tidak akan pernah menangkapnya.

---

## 11. Diagnosis terstruktur dari kajian medis — `INT-DOK-10` ★ baru pada `0.4.0`

Sembilan skenario, **lima** di antaranya jalur gagal, dan **dua** di antaranya regresi.

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `CAP-022` aturan 5 | Dokter menambah diagnosis berkode ICD dari layar kajian medis, menyebut perawatan rawat inap, **tanpa nomor konsultasi**, ketika pasien itu belum punya satu pun catatan harian | Integration | `201`; barisnya tersimpan dengan kolom konsultasi **kosong** dan kolom perawatan **terisi** |
| `CAP-022` aturan 2 | Diagnosis yang baru dibuat terbaca pada daftar masalah kajian medis pasien itu | Integration | Muncul pada pembacaan daftar yang disaring perawatan pasien tersebut |
| `INT-DOK-10` | Diagnosis dibuat **dengan** nomor konsultasi seperti sebelum `0.4.0` | **Regression** | `201`; perilakunya identik dengan sebelum perubahan. **Jalur lama tidak boleh ikut berubah** |
| `VAL-DOK-38` | **Gagal:** diagnosis tanpa nomor konsultasi pada kunjungan **rawat jalan** | **Regression** | `400`, dan **kalimat penolakannya dibandingkan utuh** dengan kalimat sebelum `0.4.0` — cara yang sama dipakai `BE-RWI-043` membuktikan `RWI-AC-143` |
| `VAL-DOK-36` | **Gagal:** nomor konsultasi dan perawatan sama-sama kosong | Integration | `400`; **nol baris tersimpan**, dan **nol konsultasi terbentuk** — dibuktikan dengan menghitung baris konsultasi sebelum dan sesudah |
| `VAL-DOK-37` | **Gagal:** nomor konsultasi dan perawatan sama-sama terisi tetapi milik pasien yang berbeda | Integration | `400`; nol baris tersimpan |
| `VAL-DOK-40` | **Gagal:** perawatan yang disebut milik pasien lain | Integration | `400`; nol baris tersimpan |
| `VAL-DOK-39` | **Gagal:** dokter yang **bukan** DPJP pasien itu menambah diagnosis dari kajian medis, walaupun butir hak akses `PatientDiagnosis : Create` dimilikinya | Integration | `403`. **Penjaga kewenangan per pasien, bukan penjaga hak akses** — mesin hak akses akan meloloskannya |
| `VAL-DOK-11` | Kajian medis diselesaikan ketika diagnosis kerja berupa teks **kosong** tetapi daftar masalah terstruktur **terisi** | Integration | `200`; kajian selesai. Membuktikan `0.4.0` tidak memperketat aturan lama |

> **Baris ketiga dan keempat adalah jaring pengaman amendment ini, dan keduanya regresi.** Yang
> paling mungkin rusak dari pelonggaran sebuah kolom wajib bukanlah jalur barunya, melainkan jalur
> lama yang selama ini bergantung pada kewajiban itu. Poliklinik memakai grup diagnosis ini setiap
> hari.
>
> **Baris kelima menghitung baris konsultasi sebelum dan sesudah**, bukan sekadar memeriksa kode
> `400`. Alasannya ditulis pada `integration-contract.md` bagian 11: cara termudah membuat jalur ini
> "berhasil" adalah diam-diam membuatkan konsultasi bayangan, dan test yang hanya melihat kode
> balasan tidak akan pernah menangkapnya.
>
> **Penomoran `RWI-AC-*` sengaja tidak dipakai di bagian ini.** Registry `RWI-AC-001` s.d.
> `RWI-AC-180` penuh, dan `RWI-AC-181` dan seterusnya sudah **dipesan** untuk penyerapan keputusan
> deposit pada `episode-rawat-inap`. Kesembilan skenario di atas karena itu ditambatkan pada aturan
> validasi dan integrasinya sendiri. Nomor `RWI-AC-*` diberikan `grill-me` bila pemilik memilih
> menerbitkan keputusan bernomor untuk `INT-DOK-10`.

---

## 10. Yang belum dapat diuji

| Butir | Kenapa belum | Kapan dapat diuji |
| --- | --- | --- |
| Nilai batas waktu verifikasi CPPT | Menunggu Clinical Governance | Mekanismenya **sudah** dapat diuji sekarang — bagian 3 |
| Nilai batas waktu kajian medis | `RWI-RULE-021` menunggu pemilik klinis | Sama |
| Pencatatan visite atas nama dokter | Kebijakannya belum ada | Setelah kebijakan ditetapkan; bawaan sekarang aman |
| Agregasi tarif visite oleh Billing | Kebijakan agregasi milik Billing belum ada | Setelah kebijakannya turun. `RWI-AC-156` tetap dapat diuji dari sisi klinis: riwayat tidak boleh berubah |
| Pembacaan balik status penyerahan obat pulang | Kontrak status final Farmasi belum disetujui pemiliknya | Setelah `RWI-DOK-RQG-003` selesai |
| Diagnosis tanpa nomor konsultasi pada kunjungan **IGD** ★ `0.4.0` | Sengaja di luar scope — pemiliknya berbeda, `RWI-DEC-069` | Setelah pemilik `EmergencyInstallationManagement` memutuskan. `integration-contract.md` bagian 10.2 |

---

## 12. Ruang kerja, kewenangan penulis, dan verifikasi — `contract_version` `0.6.0` ★ 15 September 2026

**Status `draft`.** Setiap skenario memakai data samaran. "Integration" berarti test yang memanggil endpoint beserta
basis data uji; "Regression" berarti test yang membuktikan perilaku poliklinik atau IGD tidak berubah; "UI" berarti
pemeriksaan layar dengan peran nyata non-SuperAdmin.

### 12.1 Daftar pasien dan tata letak — `RWI-DEC-107`, `RWI-DEC-111`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-DEC-111` | dr. Ahmad DPJP Budi dan konsulen Sari; 120 pasien lain dirawat. `GET census?assignedToMe=true` | Integration | Dua baris; `summary.TotalPatients = 2`; kartu Sari menampilkan DPJP dr. Rina |
| `RWI-DEC-111` | **Gagal:** permintaan mengirim `assignedToMe=true&doctorId=<dr. Rina>` dari akun dr. Ahmad | Integration | Hasil tetap pasien dr. Ahmad; `doctorId` diabaikan |
| `RWI-DEC-111` | Penugasan dokter jaga dr. Yoga atas Joko 22.00–07.00 | Integration | Joko ada pada daftar pukul 23.00; hilang pukul 07.01 |
| `RWI-DEC-111` | **Gagal:** akun tanpa tautan dokter membuka daftar | Integration | `403` |
| `UI-AC-DOK-001` s.d. `012` | Tangkapan layar Rawat Jalan dan Rawat Inap dengan teks disamarkan, pada lebar desktop, tablet, dan ponsel | UI | Kotak tata letak, lebar panel kiri, tinggi kepala, posisi tab, dan keadaan kosong sama; perbedaan hanya isi |
| `BR-RWI-002` | Ruang kerja dokter rawat inap tidak menampilkan tombol Panggil, Lewati, Tidak Hadir, maupun status Menunggu Dokter | UI | Pencarian teks pada layar dan source komponen rawat inap → nol |
| `UI-AC-DOK-007` | Belum ada pasien dipilih | UI | Judul "Belum Ada Pasien Dipilih" dengan struktur `EmptyState` Rawat Jalan |
| `AC-RWI-018` | **Gagal:** alergi gagal dimuat | UI | Kepala konteks menampilkan "Alergi gagal dimuat", **bukan** "Tidak ada alergi" |

### 12.2 Kewenangan penulis — `INV-DOK-14`, `INV-DOK-15`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-184`, `VAL-DOK-42` | **Gagal:** penugasan dr. Yoga berakhir 07.00; 08.10 membuat catatan baru berwaktu klinis 05.00 | Integration | `403`; nol baris konsultasi baru; nol registrasi |
| `RWI-AC-184` | Penugasan singkat 08.30–09.30 dibuat kepala ruangan; 08.40 kiriman ulang | Integration | `201`; waktu klinis 05.00; registrasi `Draft` terbentuk |
| `RWI-AC-185` | Konsep SOAP Joko dibuat 06.30 saat penugasan aktif; diselesaikan 08.00 tanpa penugasan | Integration | `200`; `ClinicalDateTime` 06.30 dan `SignedAt` 08.00 tersimpan terpisah |
| `RWI-AC-186`, `VAL-DOK-41` | **Gagal:** dr. Rina, DPJP aktif Joko, `PATCH /complete` konsep dr. Yoga | Integration | `403`; registrasi tetap `Draft`; `SignedByUserId` kosong |
| `RWI-AC-186` | **Gagal:** pengguna lain `PATCH /soap` konsep dr. Yoga | Integration | `403`; isi konsep tidak berubah |
| `RWI-AC-186` | **Gagal:** pengguna lain `PUT` kajian medis konsep dr. Yoga | Integration | `403` |
| `RWI-AC-186` | Konsep SOAP **poliklinik** diselesaikan pengguna lain berhak `Update` | Regression | Perilaku sama dengan sebelum `0.6.0` — dicatat apa adanya sebagai perilaku lama yang tidak disentuh |
| `VAL-DOK-58` | **Gagal:** tindakan baru dari catatan dokter pada episode `Closed` | Integration | `422`; menutup `RLN3-CAP-35` |
| `RWI-AC-189` | **Gagal:** penugasan singkat tanpa waktu selesai | Integration | Ditolak oleh `episode-rawat-inap` `VAL-INP-01` |
| `RWI-AC-190`, `VAL-DOK-44` | **Gagal:** dr. Rina dengan penugasan singkat `LateDocumentation` memverifikasi CPPT Budi | Integration | `403` |

### 12.3 Registrasi sejak konsep dan penguncian — `INV-DOK-18`, `INT-DOK-13`, `INT-DOK-15`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-215` | Konsep SOAP rawat inap dibuat, lalu simpan otomatis dua kali dan kiriman ulang dengan kunci yang sama | Integration | Tepat satu baris `MrcClinicalDocumentIntegrity` `Draft` untuk `(Consultation, Id)` |
| `RWI-AC-215` | Konsep kajian medis rawat inap dibuat | Integration | Tepat satu registrasi `(Assessment, Id)` `Draft` |
| `RWI-AC-216` | Penulis menyelesaikan konsep | Integration | Registrasi yang sama `Signed`; hitungan registrasi dokumen itu tetap 1 |
| `RWI-AC-217` | Penulis membatalkan konsep | Integration | Registrasi `Cancelled`; baris tidak dihapus |
| `RWI-AC-218` | Konsep SOAP poliklinik dan IGD dibuat | Regression | Nol registrasi sampai final |
| `RWI-AC-199` | Episode dengan satu konsep SOAP dan satu konsep kajian medis ditutup | Integration | Kedua registrasi `LockedUnsigned`, `LockTrigger = EncounterClosed`; penutupan `200` |
| `RWI-AC-199`, `VAL-DOK-43` | **Gagal:** penulis `PUT` atau `PATCH /complete` konsep yang terkunci | Integration | `409` dengan arahan addendum |
| `RWI-AC-200` | Penulis menambah addendum pada catatan `LockedUnsigned` dari "Catatan Saya" | Integration | `201`; registrasi asli tetap `LockedUnsigned`; isi asli sama byte demi byte |
| `RWI-AC-201` | Episode dibuka kembali lewat sesi koreksi | Integration | Registrasi tetap `LockedUnsigned` |
| `INT-DOK-13` | **Gagal teknis:** layanan penguncian dibuat melempar galat pada test | Integration | Penutupan `500`; episode tetap `DischargePending`; nol pesanan dibatalkan |
| `INT-DOK-13` | Penutupan dikirim ulang setelah berhasil | Integration | `200` atau `409` sesuai perilaku penutupan yang ada; nol perubahan tambahan |

### 12.4 "Catatan Saya" — `INT-DOK-14`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-181` | dr. Yoga yang penugasannya berakhir menambah addendum pada SOAP final miliknya | Integration | `201`; addendum tampil pada lini masa episode |
| `RWI-AC-182` | Daftar `my-unsigned?serviceContext=Inpatient` dan `my-authored` milik dr. Yoga | Integration | Hanya baris `AuthorUserId` dr. Yoga; nol kolom resep, pesanan, hasil |
| `RWI-AC-183` | **Gagal:** addendum jalur penulis pada SOAP dr. Rina | Integration | `403` |
| `RWI-AC-210` | Halaman Rekam Medis dan ruang kerja dokter membaca konsep penulis yang sama | Integration | Konsep rawat inap yang sama pada kedua daftar; catatan poliklinik hanya pada halaman Rekam Medis |
| `RWI-AC-211` | Pencarian endpoint daftar catatan-milik-penulis pada `ClinicalManagement` dan `InPatientManagement` | Statis | Nol |
| `INT-DOK-14` | **Gagal:** mesin keutuhan tidak terjangkau | UI | "Catatan Saya gagal dimuat" dan Coba Lagi; **bukan** daftar kosong |

### 12.5 Verifikasi CPPT — `INV-DOK-16`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-DEC-125` | DPJP aktif memverifikasi entri perawat | Integration | `200`; `VerifiedByUserId` DPJP; `ProviderUserId` tidak berubah |
| `AC-RWI-011`, `VAL-DOK-44` | **Gagal:** konsulen dengan penugasan aktif memverifikasi | Integration | `403` |
| `VAL-DOK-44` | **Gagal:** dokter jaga dengan penugasan aktif memverifikasi | Integration | `403` |
| `RWI-DEC-125` jalur 2 | DPJP dialihkan dari dr. Rina ke dr. Ahmad Jumat 07.00; Senin dr. Rina memverifikasi | Integration | `403`; dr. Ahmad `200` |
| `RWI-DEC-126` | Episode ditutup Senin 13.00; DPJP terakhir memverifikasi entri Sabtu 21.00 | Integration | `200`; daftar pantau kepatuhan tetap mencatat terlambat |
| `VAL-DOK-44a` | **Gagal:** DPJP sebelum DPJP terakhir memverifikasi pada episode `Closed` | Integration | `403` |
| `VAL-DOK-44b` | **Gagal:** entri yang ditulis setelah penutupan diverifikasi | Integration | `422` |
| `RWI-DEC-126` butir (5) | `GET verification-worklist` DPJP terakhir | Integration | Memuat "Budi Santoso — episode ditutup — 2 entri"; hilang setelah keduanya diverifikasi |

---

## 13. Pesanan, resep, rekonsiliasi, dan sliding scale — `contract_version` `0.6.0`

### 13.1 Pesanan tindakan dan instruksi — `INV-DOK-17`, `INT-DOK-19`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-DEC-114` | Ns. Siti membuat pesanan cek GDS atas instruksi dr. Yoga yang bertugas | Integration | `201`; `ConsultationId` kosong; `OrderedByUserId` Siti; `InstructingDoctorId` dr. Yoga; verifikasi `Pending` |
| `VAL-DOK-46` | **Gagal:** perawat tanpa pemberi instruksi | Integration | `400`; nol pesanan |
| `VAL-DOK-47` | **Gagal:** pemberi instruksi dr. Ahmad yang tidak bertugas atas Budi | Integration | `403`; nol pesanan |
| `VAL-DOK-45a`, `RWI-AC-143` | **Gagal:** pesanan tindakan poliklinik tanpa `ConsultationId` | Regression | `400` dengan pesan lama |
| `RWI-AC-202`, `VAL-DOK-48` | **Gagal:** dr. Ahmad, konsulen, mengubah pesanan kateter dr. Rina | Integration | `403`; isi pesanan tidak berubah |
| `RWI-AC-202` | **Gagal:** dr. Yoga mengubah isi pesanan cek GDS Ns. Siti | Integration | `403` |
| `RWI-AC-203` | Ns. Siti menandai kateter dilaksanakan | Integration | `PerformedByUserId` Siti; registrasi `(Procedure, Id)` `Signed` atas nama Siti |
| `RWI-AC-203` | **Gagal:** dr. Rina menambah addendum pada catatan pelaksanaan Siti | Integration | `403` |
| `RWI-AC-212` | DPJP aktif membatalkan pesanan konsulen dengan alasan | Integration | `200`; `CancelledByUserId` DPJP |
| `RWI-AC-212`, `VAL-DOK-49` | **Gagal:** dokter jaga membatalkan pesanan konsulen | Integration | `403` |
| `VAL-DOK-49a` | **Gagal:** pembatalan tanpa alasan | Integration | `400` |
| `RWI-AC-213` | Penutupan episode dengan dua pesanan belum dilaksanakan dan belum ditagih | Integration | Keduanya `Cancelled`, `CancelledByEpisodeClosure = true`, alasan tetap; penutupan `200` |
| `RWI-DEC-143` (c) | Penutupan episode dengan satu pesanan belum dilaksanakan tetapi `IsBillingGenerated = true` | Integration | Pesanan tidak dibatalkan; muncul pada daftar pantau episode; penutupan tidak tertahan |
| `RWI-AC-214` | Setelah penutupan | Integration | Nol pesanan dengan status registrasi "Tidak Ditandatangani" |
| `VAL-DOK-50` | dr. Yoga memverifikasi pesanan Siti; dr. Rina mencoba memverifikasi pesanan yang sama | Integration | dr. Yoga `200`; dr. Rina `403` |
| `RWI-DEC-114` (b) | Pesanan laboratorium dan radiologi perawat dengan pemberi instruksi | Integration | **Belum dapat diuji** sampai persetujuan pemilik Lab/Rad — bagian 14 |

### 13.2 Resep Harian dan penghentian butir — `RWI-DEC-121`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-DEC-121` | `GET prescriptions/episodes/{id}?period=week` hari rawat ke-4 | Integration | Dua resep, racikan terbaca, obat pulang terbedakan |
| `RWI-DEC-121` | dr. Rina menghentikan Ceftriaxone dengan alasan | Integration | `IsStopped = true`; dosis `Due` 20.00 `Cancelled` "resep dihentikan"; dosis 08.00 `Administered` utuh — satu transaksi |
| `VAL-DOK-51` | **Gagal:** dokter tanpa penugasan menghentikan butir | Integration | `403`; nol dosis berubah |
| `VAL-DOK-51a` | **Gagal:** tanpa alasan | Integration | `400` |
| `VAL-DOK-51b` | **Gagal:** menghentikan butir yang sudah dihentikan | Integration | `409` |
| `RWI-DEC-121` butir (5) | Perawat membuka Obat & Alkes | UI | Daftar sama; nol tombol hentikan; permintaan langsung perawat `403` karena tanpa `Prescription : Stop` |

### 13.3 Template resep — `RWI-DEC-122`, `RWI-DEC-135`, `INT-DOK-22`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-195`, `VAL-DOK-56` | **Gagal:** membuat template dengan `OwnerDoctorId` dr. Rina dari akun dr. Ahmad — lewat poliklinik | Integration | `403`; nol baris |
| `RWI-AC-195` | **Gagal:** sama, lewat ruang kerja rawat inap | Integration | `403`; nol baris |
| `VAL-DOK-56a` | **Gagal:** dr. Ahmad mengubah "Pneumonia dewasa" milik dr. Rina | Integration | `403`; template tidak berubah |
| `VAL-DOK-56b` | **Gagal:** template tanpa obat dan tanpa racikan | Integration | `400`; menutup `RLN3-CAP-44` |
| `RWI-AC-195` | Ruang kerja rawat inap dr. Rina `GET ?ownerScope=Mine` | Integration | Hanya "Pneumonia dewasa"; "ISPA anak" Bersama milik dr. Ahmad tidak tampil |
| `RWI-AC-195` | Poliklinik dr. Rina tanpa `ownerScope` | Regression | Template Bersama tetap tampil |
| `VAL-DOK-56c` | **Gagal:** dr. Rina memakai "ISPA anak" pada resep rawat inap lewat permintaan langsung | Integration | `403` |
| `VAL-DOK-56c` | **Gagal:** perawat memakai template pada resep rawat inap | Integration | `403` |
| `RWI-DEC-122` butir (4) | Pakai template pada pasien alergi Paracetamol | Integration | Tiga butir masuk draft; butir Paracetamol bertanda `AllergyConflict` |
| `VAL-DOK-57` | **Gagal:** menyimpan draft dengan butir bertanda | Integration | `400` menyebut Paracetamol |
| `RWI-DEC-122` butir (5) | Pakai template dengan satu obat `IsActive = false` | Integration | Butir itu bertanda `Unavailable`; butir lain masuk draft; menutup `RLN3-CAP-43` |

### 13.4 Rekonsiliasi obat — `RWI-DEC-132` s.d. `134`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-192` | Perawat mencatat Amlodipin; dokter `ContinueSame` | Integration | Butir draft resep 1×1 terbentuk; `ResultPrescriptionItemId` terisi |
| `RWI-AC-192`, `VAL-DOK-52` | **Gagal:** perawat mengambil keputusan | Integration | `403` |
| `RWI-AC-192` | Pencarian tabel rekonsiliasi pada `ClinicalManagement` dan `InPatientManagement` | Statis | Nol |
| `RWI-DEC-132` (b) | Dokter `ContinueSame` tetapi tidak menyelesaikan resep | Integration | Butir tetap draft; nol dosis MAR |
| `VAL-DOK-52a` | **Gagal:** mengganti keputusan setelah resep hasilnya aktif | Integration | `409`; keputusan lama tetap berlaku |
| `RWI-DEC-132` | Mengganti `ContinueSame` menjadi `Stopped` selama draft | Integration | Baris keputusan kedua menunjuk yang pertama; butir draft dikeluarkan dari draft |
| `RWI-AC-193` | Rilis yang memuat MAR | UI | Menu rekonsiliasi tidak berlabel "Integrasi belum tersedia" |
| `RWI-AC-194`, `VAL-DOK-53` | **Gagal:** rekonsiliasi dengan nama obat teks bebas tanpa `DrugId` | Integration | `400` |
| `RWI-AC-194` | Pendaftaran non-formularium mengirim `IsFormulary = true` | Integration | Tersimpan `IsFormulary = false` |
| `RWI-DEC-133` | Admisi → catat obat bawaan → keputusan dokter → resep aktif → dosis pertama MAR | UAT | Ketiga langkah tersedia dalam satu rilis |

### 13.5 Sliding scale — protokol dan order

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-222`, `VAL-DOK-55` | **Gagal:** memesan dari versi `Draft` | Integration | `409` |
| `RWI-AC-223`, `VAL-DOK-55a` | **Gagal:** menyesuaikan dosis tanpa alasan | Integration | `400` |
| `RWI-AC-223`, `VAL-DOK-54` | **Gagal:** penyesuaian 200–260 dan 250–299 | Integration | `400` menyebut kedua rentang |
| `VAL-DOK-54a` | **Gagal:** versi template tanpa rentang di bawah 150 | Integration | `400` |
| `RWI-AC-224` | Order Budi memakai v2; v3 disahkan | Integration | Order tetap menunjuk v2; pelaksanaan lama tetap `OrderVersionId` lamanya |
| `VAL-DOK-54c` | **Gagal:** pengubah terakhir mengesahkan | Integration | `403` |
| `RWI-DEC-146` butir (1) | Pengguna lain mengesahkan v2 | Integration | v2 `Approved`, v1 `Retired`, satu transaksi; unique parsial tidak dilanggar |
| `RWI-AC-225` | Pencarian tabel sliding scale pada `ClinicalManagement` dan `InPatientManagement` | Statis | Nol |
| `RWI-AC-226` | Order dihentikan; pelaksanaan berikutnya dicoba | Integration | Ditolak pada kontrak `keperawatan` — dirujuk |
| `VAL-DOK-55c` | **Gagal:** dua dokter menyesuaikan order yang sama dengan nomor versi sama | Integration | Satu `201`, satu `409` |
| `RWI-DEC-121` × `RWI-DEC-147` | Butir insulin berdosis skala dihentikan | Integration | Order `Stopped` dalam transaksi yang sama |
| `VAL-DOK-55e` | Belum ada protokol `Approved` | UI | "Belum ada protokol sliding scale yang disahkan"; nol form yang dapat disimpan |

### 13.6 Jenis catatan CPPT — `RWI-DEC-140`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-204` | Perawat menulis narasi `NursingNarrative` | Integration | Satu baris CPPT profesi Perawat; nol tabel Catatan Keperawatan |
| `RWI-AC-205` | `GET episodes/{id}?noteKind=NursingSoap` dan `?noteKind=NursingNarrative` | Integration | Masing-masing hanya jenisnya; tanpa saring memuat keduanya beserta jenis |
| `VAL-DOK-59` | **Gagal:** perawat mengirim `PhysicianNote` | Integration | `400` |
| `VAL-DOK-59a` | **Gagal:** akun tanpa tautan profesi | Integration | `403` |
| `RWI-DEC-140` (1) | Migration pada basis data berisi CPPT lama | Migration | Seluruh baris lama `NoteKind = 0`; nol baris diisi tebakan |

### 13.7 Resume Medis dan penunjang — `INT-DOK-20`, `RWI-DEC-123`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-DEC-112` | DPJP menyimpan draf dan menandatangani dari tab Resume Medis | Integration + UI | `FE-INP-06` menampilkan resume yang sama; episode belum `Closed` |
| `RWI-DEC-112` | **Gagal:** konsulen menandatangani dari tab | Integration | `403` dari penjaga episode |
| `RWI-DEC-123` | Membuka Resume ODC | UI | "Integrasi belum tersedia"; nol form; nol aksi |
| `RWI-DEC-113` | Penunjang Gizi, Hemodialisa, Bank Darah, Rehab Medik | UI | Konteks pasien + "Integrasi belum tersedia"; nol data contoh; nol permintaan jaringan ke modul itu |

---

## 14. Yang belum dapat diuji pada `0.6.0`

| Butir | Kenapa | Kapan |
| --- | --- | --- |
| Pesanan laboratorium dan radiologi dengan pemberi instruksi | Persetujuan pemilik `LaboratoryManagement` dan `RadiologyManagement` belum tercatat | Setelah persetujuan tercatat |
| `my-authored` untuk catatan terkunci | Menunggu persetujuan Yoga Aji Pratama | Setelah persetujuan |
| Kesamaan tata letak penuh | Menunggu persetujuan pemilik `rawat-jalan` atas ekstraksi komponen — `INT-DOK-21` | Setelah persetujuan |
| Catatan terlambat yang belum pernah dimulai | Jalur tulis penugasan singkat milik `episode-rawat-inap` `0.9.0` belum dibangun | Setelah task episode selesai |
| Pemakaian sliding scale untuk pasien sungguhan | Isi template belum disahkan pemilik klinis yang belum ditunjuk | Gerbang produksi |
| Batas waktu verifikasi instruksi dan verifikasi CPPT | `RWI-RULE-021` belum final | Gerbang produksi |
