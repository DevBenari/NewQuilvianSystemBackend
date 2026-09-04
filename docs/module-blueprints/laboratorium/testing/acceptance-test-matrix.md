# Acceptance Test Matrix — Modul Laboratorium

| Field | Value |
|---|---|
| Blueprint ID | `LAB-BP-001` |
| Revision | `3` |
| Status | `draft` |
| Scope | Slice `S1a`, `S2`, `S3`, `S7`, `S10`, `S11`, `S13a`, `S13b`, `S14`, `S15` |
| Backend SHA | `c87d9c0` |
| Frontend SHA | `688daff90` |
| Contract version | `LAB-API-v1` r3, `LAB-STATE-v1` r2, `LAB-VAL-v1` r3, `LAB-INT-v1` r3, `LAB-PERM-v1` r3 — `approved` 2026-09-02 |

Matriks ini memuat **jalur gagal**, bukan hanya jalur berhasil. Pengujian yang hanya membuktikan
jalur berhasil tidak membuktikan apa pun tentang keamanan modul.

Acuan `AC-nn` berasal dari `00-interview-decisions.md`.

---

## 1. Pengujian yang Sudah Ada dan Wajib Tetap Lulus

Tiga puluh pengujian pada `c87d9c0` sudah membuktikan sebagian invariant. Pekerjaan ini
**tidak boleh** memecahkannya; yang boleh berubah hanyalah satuan datanya.

| Berkas | Jumlah | Yang dibuktikan |
|---|---:|---|
| `Tests/QuilvianSystemBackend.BillingTests/Laboratory/LaboratorySpecimenLifecycleTests.cs` | 18 | Siklus hidup sampel, kelayakan tagih, ambil ulang, pembatalan, konkurensi |
| `Tests/QuilvianSystemBackend.BillingTests/Laboratory/LaboratoryAuthorityTests.cs` | 12 | Batas kewenangan finansial, pemisahan permission, bentuk barcode, kesesuaian enum |

> **Angka dikoreksi 2026-09-02.** Revision sebelumnya menulis 31 dan 19. Hitungan sebenarnya
> pada `HEAD` adalah **18** atribut `[Fact]` dan nol `[Theory]` pada berkas siklus hidup — berkas
> itu memuat 19 method publik, satu di antaranya method bantu, bukan pengujian. Totalnya 30.

### Pengujian lama yang **wajib disesuaikan** akibat `LAB-DEC-024`

| Pengujian | Kenapa berubah | Yang harus tetap benar |
|---|---|---|
| `#DuaKomponenLayakSatuDitolak_MenagihTigaRatusLimaPuluhRibu` | Tiga komponen kini menjadi tiga wadah berisi satu pemeriksaan, atau satu wadah berisi beberapa | Jumlah rupiah yang diserahkan tetap Rp350.000 |
| `#PenetapanLayak_MembentukTepatSatuFaktaDanSatuBarisTagihan` | Satu wadah kini dapat menerbitkan lebih dari satu fakta | Jumlah fakta sama dengan jumlah pemeriksaan yang ditopang |
| `#PengambilanUlang_MempertahankanSampelDitolakDanTautanSebabnya` | Ambil ulang kini memindahkan seluruh pemeriksaan | Wadah lama tetap terlihat dan tertaut |

---

## 1b. Alur Pemesanan Lintas Unit

Ditambahkan 2026-09-02 setelah `roadmap/traceability.md` menemukan `AC-11` tidak punya baris uji
mana pun, padahal `LAB-DEC-009` sudah menetapkannya dan `BE-LAB-01` menambah kolom pada
`LabOrder` sehingga jalur pembuatan pesanan ikut tersentuh.

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| AC-11 | Membuat pesanan lab dari kunjungan Rawat Jalan | Integration | Pesanan terbentuk; `EncounterId` menunjuk kunjungan ber-`EncounterType` `Outpatient` |
| AC-11 | Membuat pesanan lab dari kunjungan Rawat Inap | Integration | Alur kerjanya **sama persis**; tidak ada cabang khusus per jenis kunjungan |
| AC-11 | Membuat pesanan lab dari kunjungan IGD | Integration | Alur kerjanya sama persis; `EncounterType` `Emergency` |
| AC-11 | Ketiganya setelah kolom `Discipline` ditambahkan `BE-LAB-01` | Integration | Ketiga alur tetap lulus; kolom baru terisi dan tidak memaksa cabang baru |

**Kenapa ini diuji, padahal kemampuannya sudah ada.** `CAP-08` menyatakan kunjungan dari ketiga
unit sudah tersedia di tingkat data tanpa perubahan apa pun. Yang belum pernah dibuktikan adalah
bahwa **alur kerjanya benar-benar sama** untuk ketiganya, dan bahwa penambahan kolom disiplin
tidak diam-diam melahirkan cabang khusus per jenis kunjungan.

---

## 2. Slice `S1a` — Penandaan Cito

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| AC-18 | Dokter pemesan menandai pesanannya sebagai cito | Integration | `Urgency` menjadi `Cito`; `UrgencyMarkedAt` dan `UrgencyMarkedByUserId` terisi; satu baris riwayat terbentuk |
| AC-18 | **Gagal** — dokter lain mencoba menandai cito pesanan yang bukan miliknya | Integration | `403`, pesan `VAL-03`; tidak ada perubahan data |
| AC-18 | **Gagal** — menandai cito pesanan yang sudah `Completed` | Integration | `409`, pesan `VAL-04` |
| AC-18 | Mengembalikan pesanan cito menjadi biasa | Integration | `Urgency` menjadi `Routine`; riwayat bertambah satu baris |

---

## 3. Slice `S2` — Wadah dan Pemeriksaan

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| AC-35 | Merencanakan satu wadah berisi dua pemeriksaan | Integration | Satu baris wadah dengan satu barcode; dua baris pemeriksaan menunjuk wadah itu |
| AC-35 | **Gagal** — merencanakan wadah tanpa satu pun pemeriksaan | Integration | `422`, pesan `VAL-05` |
| AC-35 | **Gagal** — memasukkan jenis pemeriksaan yang sama dua kali pada satu wadah | Integration | `422`, pesan `VAL-07` |
| AC-36 | Menolak wadah yang menopang dua pemeriksaan | Integration | Wadah `Rejected`; **kedua** pemeriksaan `Voided`; tidak ada fakta terbit |
| AC-36 | **Gagal** — mencoba menolak satu pemeriksaan saja pada wadah berisi dua | Integration | `422`, pesan `VAL-13`; tidak ada perubahan status |
| AC-37 | Menyatakan layak wadah yang menopang dua pemeriksaan bertarif Rp150.000 dan Rp120.000 | Integration | Dua fakta kelayakan tagih terbit; masing-masing membawa salinan tarifnya sendiri |
| AC-37 | **Gagal** — menyatakan layak wadah yang belum pernah diterima | Integration | `409`, pesan `VAL-08`; tidak ada fakta terbit |
| AC-38 | Ambil ulang atas wadah yang ditolak | Integration | Wadah baru terbentuk, menampung seluruh pemeriksaan dari wadah lama; wadah lama tetap ada dan tertaut |
| AC-38 | **Gagal** — ambil ulang tanpa mengisi sebab | Integration | `422`, pesan `VAL-14` |
| AC-12 | Fakta kelayakan tagih terbit tepat pada perpindahan ke `Accepted` | Integration | Waktu fakta sama dengan waktu perpindahan; tidak ada fakta sebelum itu |
| `INV-05` | Dua petugas menyatakan layak wadah yang sama bersamaan | Integration | Hanya satu berhasil; yang lain `409` |
| `INV-06` | Menyatakan layak dua kali berturut-turut | Integration | Jumlah fakta tetap sama dengan jumlah pemeriksaan; tidak berlipat |
| AC-13 | Menelusuri seluruh model dan service Laboratorium | Unit | Tidak ditemukan properti maupun method finansial |

---

## 4. Slice `S3` — Batas Nilai dan Batas Kritis

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| AC-24 | Membuat tiga baris batas Hemoglobin: pria dewasa, wanita dewasa, dan anak | Integration | Ketiganya tersimpan; tidak ada yang menolak yang lain |
| AC-24 | **Gagal** — membuat baris keempat dengan kombinasi pemeriksaan, jenis kelamin, dan umur yang sama | Integration | `409`, pesan `VAL-21` |
| AC-25 | Menelusuri skema `MstProcedure` setelah seluruh migration dijalankan | Unit | Tidak ada satu pun kolom baru yang ditambahkan modul Laboratorium |
| AC-28 | Membuat batas nilai protein urin berbentuk pilihan dengan lima pilihan | Integration | Lima baris pilihan tersimpan; `P3` dan `P4` bertanda kritis |
| AC-28 | **Gagal** — membuat batas berbentuk angka tanpa satuan | Integration | `422`, pesan `VAL-22` |
| AC-28 | **Gagal** — membuat batas berbentuk pilihan tanpa satu pun pilihan | Integration | `422`, pesan `VAL-23` |
| AC-28 | **Gagal** — membuat batas berbentuk angka disertai daftar pilihan | Integration | `422`, pesan `VAL-24` |
| — | **Gagal** — batas kritis bawah 4,0 pada Kalium bernormal 3,5–5,1 | Integration | `422`, pesan `VAL-26` |
| AC-33 | Kepala instalasi mengubah batas normal Hemoglobin | Integration | Perubahan langsung berlaku; satu baris riwayat terbentuk tanpa penyetuju |
| AC-33 | Kepala instalasi mengajukan perubahan batas kritis Kalium dari 6,0 menjadi 8,0 | Integration | Pengajuan `Submitted`; **batas pada `LabValueBound` tidak berubah** |
| AC-33 | **Gagal** — mencoba mengubah batas kritis lewat endpoint ubah biasa | Integration | `422`, pesan `VAL-28`; batas tidak berubah |
| AC-33 | Pihak klinis menyetujui pengajuan | Integration | Batas kritis berubah menjadi 8,0; riwayat terbentuk dengan penyetuju terisi |
| AC-33 | **Gagal** — pengaju menyetujui pengajuannya sendiri | Integration | `403`, pesan `VAL-33`; batas tidak berubah |
| AC-33 | **Gagal** — mengajukan perubahan kedua saat pengajuan pertama belum diputuskan | Integration | `409`, pesan `VAL-32` |
| AC-34 | Menelusuri riwayat setelah beberapa perubahan | Integration | Setiap perubahan punya kolom, nilai lama, nilai baru, pelaku, waktu, dan alasan |

---

## 5. Slice `S7` — Daftar Kerja dan Keterlambatan Cito

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| AC-10 | Empat belas pesanan biasa masuk pukul 10.00, satu pesanan cito masuk pukul 10.05 | Integration | Pesanan cito berada di urutan pertama daftar kerja, bukan urutan kelima belas |
| AC-10 | Dua pesanan cito dengan waktu masuk berbeda | Integration | Keduanya di atas pesanan biasa; di antara keduanya diurutkan menurut waktu masuk |
| AC-17 | Kalium cito berbatas 60 menit, wadah layak pukul 09.00, belum dirilis sampai pukul 10.20 | Integration | Muncul pada daftar pantau keterlambatan dengan kelebihan waktu 20 menit |
| AC-17 | Kalium cito berbatas 60 menit, selesai pukul 09.45 | Integration | **Tidak** muncul pada daftar pantau keterlambatan |
| `VAL-39` | Pesanan cito untuk pemeriksaan yang belum punya batas waktu cito | Integration | Tidak dianggap terlambat; ditampilkan berketerangan batas waktu belum diatur |
| — | **Gagal** — pengguna tanpa kewenangan membuka daftar kerja | Integration | `403` |

---

## 6. Slice `S10` — Fakta Kelayakan Tagih

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| AC-37 | Satu wadah dua pemeriksaan dinyatakan layak | Integration | Dua fakta `ChargeEligibility`; `SourceItemId` menunjuk identitas **pemeriksaan**, bukan wadah |
| AC-12 | Wadah ditolak | Integration | Tidak ada fakta apa pun terbit |
| — | Pembatalan pemeriksaan yang sudah layak tagih | Integration | Satu fakta `ClinicalCancellation` terbit; tagihan **tidak** dihapus oleh Laboratorium |
| — | Pembatalan pemeriksaan yang belum pernah layak tagih | Integration | Fakta berstatus `SuppressedNoPriorCharge`, atau tidak terbit sama sekali |
| — | Ambil ulang karena kesalahan internal rumah sakit | Integration | Jumlah fakta kelayakan tagih untuk pemeriksaan itu tetap satu, bukan dua |
| AC-13 | Menelusuri isi fakta yang diterbitkan | Unit | Tidak memuat keputusan tagihan, status pembayaran, refund, maupun pembalikan |

---

## 7. Slice `S11` — Alasan Penolakan Sampel

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| AC-26 | Kepala instalasi menambah alasan "Sampel tidak diberi label" | Integration | Alasan tersimpan; penanda kesalahan internal bernilai bawaan, tidak dapat diisi dari permintaan |
| AC-26 | **Gagal** — kepala instalasi mencoba mengubah penanda kesalahan internal | Integration | `403`, pesan `VAL-37`; penanda tidak berubah |
| AC-26 | Administrator sistem menyetel penanda kesalahan internal | Integration | Penanda berubah; tercatat pada logger |
| AC-26 | **Gagal** — menambah alasan dengan kode yang sudah dipakai | Integration | `409`, pesan `VAL-36` |
| AC-26 | **Gagal** — menonaktifkan alasan terakhir yang masih aktif | Integration | `422`, pesan `VAL-38` |
| — | Menolak wadah memakai alasan yang menuntut catatan, tanpa mengisi catatan | Integration | `422`, pesan `VAL-12` |

---

## 7b. Slice `S1a` — Cito dan Duplo pada Pemeriksaan

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| AC-39 | Satu pesanan berisi Kalium bertanda cito dan Kolesterol biasa | Integration | Hanya Kalium naik ke urutan atas daftar kerja; Kolesterol tetap di antrean biasa |
| AC-40 | **Gagal** — mencoba menyetel penanda cito pada pesanan, bukan pemeriksaan | Integration | Ditolak; tidak ada endpoint kesegeraan pada grup Lab Order |
| AC-40 | Menandai satu pemeriksaan dikerjakan ganda | Integration | `IsDuplo` bernilai benar pada baris pemeriksaan itu saja |
| — | **Gagal** — dokter lain menandai cito pemeriksaan pada pesanan yang bukan miliknya | Integration | `403`, pesan `VAL-03` |

---

## 7c. Slice `S13a` dan `S13b` — Pendaftaran Pasien

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| AC-44 | Mendaftarkan pasien datang langsung dari layar Laboratorium | Integration | Kunjungan terbentuk oleh Registrasi dengan `IsWalkIn` benar dan sumber pendaftaran `WalkIn`; pesanan lab menempel padanya |
| AC-45 | Menelusuri seluruh kode Laboratorium | Unit | Tidak ditemukan satu pun penulisan ke tabel kunjungan maupun tabel pasien |
| AC-46 | Mendaftarkan pasien rujukan luar | Integration | Kunjungan menyimpan penunjuk instansi perujuk, penunjuk dokter perujuk, dan nomor surat rujukan |
| AC-50 | **Gagal** — mengetik nama instansi perujuk sebagai teks bebas | Integration | `422`, pesan `VAL-43`; petugas diarahkan memilih dari daftar |
| — | **Gagal** — mendaftarkan pasien rujukan tanpa nomor surat rujukan | Integration | `422`, pesan `VAL-44` |
| — | **Gagal** — Registrasi menolak karena kewenangan | Integration | `403`; **tidak ada** data yang tersimpan di Laboratorium |
| — | **Gagal** — Registrasi tidak dapat dihubungi | Integration | `503`; tidak ada kunjungan setengah jadi, tidak ada pesanan yatim |
| — | Permintaan pendaftaran yang sama dikirim dua kali | Integration | Satu kunjungan saja; permintaan kedua mengembalikan kunjungan yang sama |

**Skenario yang paling penting dibuktikan.** Yang terakhir — kirim ganda. Bila gagal, satu
pasien mendapat dua kunjungan pada hari yang sama, pesanan lab terbelah, hasil tersebar, dan
Billing menerima dua konteks tagihan.

---

## 7d. Slice `S14` — Katalog, Harga, dan Cakupan

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| AC-43 | Membuka layar pemesanan dan memilih tiga pemeriksaan | Integration | Harga satuan, subtotal, dan total tampil; **tidak ada** baris tagihan yang terbentuk |
| AC-47 | Menelusuri seluruh tabel milik Laboratorium | Unit | Tidak ditemukan tabel tarif; harga selalu berasal dari `MstTariff` |
| AC-48 | **Gagal** — mengubah tarif lewat endpoint modul Laboratorium | Integration | `403`, pesan `VAL-50`; tidak ada endpoint tulis pada grup Lab Catalog |
| — | Pemeriksaan tanpa kontrak penjamin untuk penjamin pasien | Integration | Ditampilkan **tidak tercakup**; pemeriksaan **tetap dapat** dipesan |
| — | **Gagal** — pemeriksaan tanpa tarif berlaku pada tanggal kejadian | Integration | `422`, pesan `VAL-47` |
| AC-51 | **Gagal** — menambahkan Hemoglobin ke pesanan berdisiplin Mikrobiologi | Integration | `422`, pesan `VAL-46` |
| — | Pemeriksaan berpenanda laboratorium tetapi belum punya disiplin | Integration | Tidak muncul pada daftar disiplin mana pun; ada keterangan bagi kepala instalasi |

---

## 7e. Slice `S15` — Monitoring per Disiplin

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| AC-41 | Membuka tiga daftar pantau dengan data campuran | Integration | Masing-masing hanya menampilkan pesanan berdisiplin sesuai jalurnya |
| AC-42 | Menelusuri seluruh endpoint dan tabel Laboratorium | Unit | Tidak ditemukan satu pun yang melayani Bank Darah |
| AC-19 | Menelusuri seluruh tabel dan endpoint Laboratorium — **ditambahkan 2026-09-02** | Unit | Tidak ditemukan satu pun yang menyimpan stok, pembelian, maupun pemakaian reagen (`LAB-DEC-014`) |
| — | Menyaring daftar pantau menurut penjamin, status, dan penanda cito | Integration | Hasil penyaringan sesuai; penyaring sama pada ketiga jalur |
| — | **Gagal** — pengguna tanpa kewenangan membuka daftar pantau | Integration | `403` |

---

## 7f. Penempatan Berkas

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| AC-49 | Menelusuri struktur folder backend setelah implementasi | Review | `LabValueBound`, `LabValueOption`, `MstLabRejectionReason` berada di `LaboratoryManagement/Models/`; tidak ada data induk global yang disalin ke sana |
| AC-49 | Menelusuri struktur folder frontend | Review | Seluruh menu data induk berada di `health-services/master-data/`; folder `laboratory-management` hanya berisi layar operasional |

---

## 8. Pengujian Migration

| Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|
| Menjalankan `SplitLabSpecimenIntoExamination` atas basis data berisi baris sampel lama | Migration test | Setiap baris lama menjadi satu wadah dan satu pemeriksaan; tidak ada data hilang |
| Identitas baris pemeriksaan hasil pemindahan | Migration test | Sama dengan identitas sampel lama, sehingga `BilChargeLines.SourceItemId` tetap tertaut |
| Salinan tarif setelah pemindahan | Migration test | Berada pada baris pemeriksaan, bukan pada wadah |
| Barcode setelah pemindahan | Migration test | Tetap melekat pada wadah dan tetap unik |
| **Gagal** — menjalankan migration atas basis data yang jumlah barisnya belum diverifikasi | Prosedur | Dihentikan sampai `LAB-OPEN-012` dijawab |

---

## 9. Pengujian Frontend

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `LAB-FE-006` | Membuka daftar kerja berisi pesanan cito dan biasa | Component | Pesanan cito tampil di atas, tanpa memandang waktu masuk |
| `LAB-FE-009` | Membuka layar penolakan wadah berisi dua pemeriksaan | Component | Kedua pemeriksaan terlihat sebelum tombol tolak dapat ditekan |
| `LAB-FE-010` | Menekan tolak pada wadah berisi dua pemeriksaan | Component | Peringatan muncul menyebut kedua pemeriksaan akan gugur |
| `LAB-FE-011` | Mengubah batas kritis di layar batas nilai | Component | Yang tersedia adalah tombol ajukan, bukan tombol simpan |
| `LAB-FE-012` | Membuka layar alasan penolakan sebagai kepala instalasi | Component | Kolom kesalahan internal dan kolom wajib catatan tampil terkunci |
| `LAB-FE-013` | Membuka batas nilai berbentuk pilihan | Component | Isian satuan dan empat batas angka **tidak** ditampilkan |
| — | Menekan tombol menyatakan layak dua kali cepat | Component | Permintaan kedua tidak terkirim; tombol terkunci sejak penekanan pertama |
| — | Server menjawab `409` | Component | Pesan muat ulang muncul; tidak ada pengiriman ulang otomatis |
| — | Pengguna tanpa kewenangan membuka layar | Component | Tombol tindakan tersembunyi atau nonaktif, bukan gagal saat ditekan |

---

## 10. Yang Tidak Diuji pada Rilis Ini

| Yang tidak diuji | Alasan |
|---|---|
| Pengisian, validasi, dan rilis hasil | Slice `S4` terblokir `LAB-SIGN-001` |
| Penandaan dan pelaporan nilai kritis | Slice `S5` terblokir |
| Koreksi hasil dan addendum | Slice `S6` terblokir |
| Pemberitahuan tersimpan | Slice `S8` terblokir `LAB-COORD-001` |
| Pendaftaran hasil ke rekam medis | Slice `S9` terblokir `LAB-COORD-002` |
| Penyuntingan pesanan oleh dokter | Slice `S1b` terblokir `LAB-AMD-001` |

**Yang perlu disadari.** Batas nilai dan batas kritis pada `S3` sudah diuji **bentuk dan
wewenangnya**, tetapi belum diuji **pemakaiannya untuk menilai hasil** — karena hasil belum ada.
Pengujian penilaian kritis baru dapat ditulis setelah `S4` dan `S5` dibuka.
