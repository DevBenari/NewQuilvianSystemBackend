# Acceptance Test Matrix — Sub-modul `dokter-rawat-inap` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `dokter-rawat-inap` |
| Contract version | `0.3.0` |
| `last_changed_in` | `0.3.0` |
| Status | `approved` — disetujui Muhammad Hamzah, 2026-09-03 |
| `input_revision` | `02-backend-architecture.md` `0.2`; seluruh kontrak `0.2.0`; arsitektur domain `0.2` |
| `input_hash` | Arsitektur domain SHA-256 `226c6ef1e4bfec544c366b265fe1e4530e80c510da33c1a9eaf2e62161d0b717` |
| Backend SHA | `93b3227c431401d8f586dec4e1fb25fbf41766e3` |
| `approved_by` / `approved_at` | **Muhammad Hamzah** / **2026-09-03** |
| Tanggal | 2 September 2026; disetujui 3 September 2026 |

Dari **54** skenario di bawah, **22** adalah jalur gagal.

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

## 10. Yang belum dapat diuji

| Butir | Kenapa belum | Kapan dapat diuji |
| --- | --- | --- |
| Nilai batas waktu verifikasi CPPT | Menunggu Clinical Governance | Mekanismenya **sudah** dapat diuji sekarang — bagian 3 |
| Nilai batas waktu kajian medis | `RWI-RULE-021` menunggu pemilik klinis | Sama |
| Pencatatan visite atas nama dokter | Kebijakannya belum ada | Setelah kebijakan ditetapkan; bawaan sekarang aman |
| Agregasi tarif visite oleh Billing | Kebijakan agregasi milik Billing belum ada | Setelah kebijakannya turun. `RWI-AC-156` tetap dapat diuji dari sisi klinis: riwayat tidak boleh berubah |
| Pembacaan balik status penyerahan obat pulang | Kontrak status final Farmasi belum disetujui pemiliknya | Setelah `RWI-DOK-RQG-003` selesai |
