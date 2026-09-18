# Acceptance Test Matrix — Modul Laboratorium

| Field | Value |
|---|---|
| Blueprint ID | `LAB-BP-001` |
| Revision | `5` |
| Status | `draft` |
| Scope | Slice `S1a`, `S2`, `S3`, `S7`, `S10`, `S11`, `S13a`, `S13b`, `S14`, `S15`. **Revision 4 menambah amandemen Penerimaan Sampling/Specimen** — lihat bagian 11 |
| Backend SHA | Revision 1-3: `c87d9c0`. **Revision 4: `466a7127`**, diverifikasi tidak berubah pada `9067fa73` |
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

---

## 11. Amandemen 2026-09-14 — Penerimaan Sampling/Specimen

Menutup `AC-52` sampai `AC-67` dan `AC-75` sampai `AC-77` dari decision log revision 26.
`AC-68` sampai `AC-74` dan `AC-78` sampai `AC-82` **tidak diuji di sini** — keduanya menunggu
`LAB-REQ-005`.

### 11.1 ~~Qty memperbanyak baris~~ — **dicabut `LAB-DEC-050`**

> **Seluruh pengujian bagian ini dicabut pada 2026-09-14.** `BE-LAB-23` membuktikan `AC-52`
> tidak dapat dipenuhi: index unik `(SpecimenId, ProcedureId)` yang dipasang atas dasar `BR-20`
> dan `AC-35` menolak baris kedua untuk jenis pemeriksaan yang sama pada satu wadah. Pemilik
> modul mencabut `LAB-DEC-038`; kolom Jumlah tidak dibuat, dan ruas `Quantity` dicabut dari
> kontrak lewat `LAB-API-v1` `r9`.

| ID | Keadaan |
|---|---|
| ~~`T-52a`~~, ~~`T-53a`~~, ~~`T-54a`~~, ~~`T-60a`~~, ~~`T-60b`~~ | **Dicabut.** Menguji perilaku yang tidak jadi dibangun |

**Satu pengujian dipertahankan, dan justru berubah makna:**

| ID | Skenario | Jenis | Yang membuktikan |
|---|---|---|---|
| `T-52b` | Menelusuri seluruh model Laboratorium: **tidak ada satu pun** properti bernama `Qty` atau `Quantity` | Unit, refleksi | BR-45 |

Semula `T-52b` menjaga agar Qty tidak diam-diam menjadi kolom. Kini ia menjaga hal yang lebih
kuat: **Qty tidak pernah ada sama sekali**, termasuk sebagai ruas permintaan. Penjaga yang sama,
alasan yang berbeda.

**Satu pengujian baru menggantikan yang dicabut:**

| ID | Skenario | Jenis | Yang membuktikan |
|---|---|---|---|
| `T-45a` | Menambahkan jenis pemeriksaan yang sama dua kali pada satu wadah ditolak — oleh service dengan pesan `VAL-07`, dan oleh index unik bila keduanya datang bersamaan | Integrasi | BR-45, BR-20, AC-35 |

`T-45a` menjaga justru aturan yang membatalkan `LAB-DEC-038`. Tanpa penjaga itu, index uniknya
dapat dilepas seseorang di kemudian hari tanpa menyadari bahwa satu keputusan bergantung
padanya.

### 11.2 Titik kunci — `LAB-DEC-039`

| ID | Skenario | Jenis | Yang membuktikan |
|---|---|---|---|
| `T-55a` | Menambah baris **berhasil** selama kelayakan belum ditetapkan | Integrasi | AC-55 |
| `T-55b` | Menambah baris pada wadah ber-status `Accepted` ditolak `409` `VAL-18` | Integrasi | AC-55 |
| `T-55c` | Menambah baris pada wadah ber-status `Rejected` juga ditolak `409` | Integrasi | AC-55 |
| ~~`T-55d`~~ | **Dicabut `LAB-DEC-049`.** Semula: menghapus baris pada wadah yang sudah diputuskan ditolak | — |
| `T-55e` | **Membatalkan** pemeriksaan pada wadah ber-status `Accepted` **tetap berhasil**; hanya `VAL-19` yang menahannya bila pemeriksaan sudah gugur | Integrasi | AC-55, AC-57, `LAB-INH-006` |
| `T-56a` | Menelusuri seluruh controller Laboratorium: tidak ada route maupun aksi bernama `process`, `diproses`, atau sejenisnya **pada tingkat pemeriksaan** | Unit | AC-56 |

> **`T-55b` dan `T-55c` menguji perilaku yang sudah ada**, bukan perilaku baru.
> `LabExaminationService.cs:123@466a7127` sudah menegakkannya sebagai `VAL-18`. Keduanya tetap
> ditulis sebagai penjaga: `LAB-DEC-039` menaikkan perilaku itu menjadi keputusan, dan keputusan
> tanpa pengujian dapat hilang pada refactor berikutnya tanpa ada yang menyadarinya.
>
> **`T-55d` dicabut, dan `T-55e` mengambil tempatnya dengan arah yang berlawanan.** Pemeriksaan
> `BE-LAB-24` menemukan jalur batal sengaja tidak terkunci: `VAL-18` memang hanya berlaku pada
> penambahan, `LAB-INH-006` mengatur jalur pengajuan pembatalan, dan `LAB-INH-010` menyerahkan
> koreksi tagihan kepada Billing. `LAB-DEC-049` mempersempit `AC-55`/`AC-57` sesuai temuan itu.
>
> `T-55e` karena itu menguji bahwa pembatalan **tetap berhasil** — penjaga terhadap seseorang
> yang kelak menambahkan `VAL-18` ke jalur batal dengan maksud baik, lalu menutup jalur sah
> tanpa ada yang menyadarinya.

### 11.3 Jenis specimen — `LAB-DEC-040`

| ID | Skenario | Jenis | Yang membuktikan |
|---|---|---|---|
| `T-58a` | Menyimpan wadah tanpa jenis ditolak `422` `VAL-51` | Unit | AC-58 |
| `T-58b` | Mengirim jenis sebagai teks di luar jalur `Lainnya` ditolak `422` `VAL-52` | Unit | AC-58 |
| `T-58c` | Jenis yang sudah dinonaktifkan ditolak `422` `VAL-55` | Integrasi | AC-58 |
| `T-59a` | Memilih `Lainnya` tanpa keterangan ditolak `422` `VAL-53` | Unit | AC-59 |
| `T-59b` | Keterangan `Lainnya` yang dikirim bersama jenis selain `Lainnya` ditolak `422` `VAL-54` | Unit | AC-59 |
| `T-59c` | **Penerimaan wadah berjenis `Lainnya` berhasil disimpan** — tidak ada `422` yang menahannya | Integrasi | AC-59 |
| `T-60c` | Menambah jenis berkode sama ditolak `409` `VAL-61` | Integrasi | `VAL-61` |
| `T-62a` | Menyetel `IsOtherBucket` pada baris kedua ditolak `422` `VAL-62` | Integrasi | `VAL-62` |
| `T-63a` | Menonaktifkan satu-satunya jenis `Lainnya` yang aktif ditolak `422` `VAL-63` | Integrasi | `VAL-63` |
| `T-60d` | `GET /other-usage` mengembalikan keterangan `Lainnya` beserta jumlah dan pemakaian terakhirnya | Integrasi | AC-60 |
| `T-61a` | Wadah lama yang hanya punya `SpecimenDescription` tetap terbaca setelah migration, dan tidak dihapus | Integrasi | AC-61 |

`T-59c` adalah pengujian yang paling mudah lupa ditulis: seluruh pengujian lain memastikan sistem
**menolak**, sedangkan yang ini memastikan sistem **tidak menolak**. `LAB-DEC-040` memilih
`Lainnya` justru agar sampel tidak tertahan; tanpa `T-59c`, satu validasi yang terlalu ketat
dapat mengembalikan jalan buntu itu tanpa ada pengujian yang gagal.

### 11.4 Volume — `LAB-DEC-041`

| ID | Skenario | Jenis | Yang membuktikan |
|---|---|---|---|
| `T-62b` | Volume dikirim tanpa satuan ditolak `422` `VAL-56` | Unit | AC-62 |
| `T-62c` | Satuan yang tidak ber-`IsForLaboratory` ditolak `422` `VAL-57` | Integrasi | AC-62 |
| `T-63b` | Volume bernilai sangat kecil **diterima** tanpa peringatan maupun penolakan | Unit | AC-63 |
| `T-63c` | Tidak ada satu pun jalur kode yang membandingkan volume terhadap batas minimum | Unit, refleksi | AC-63, `RULE-021` |
| `T-64a` | Specimen berjenis Jaringan dapat menyimpan volume bersatuan `gram`, `blok`, atau `slide` | Integrasi | AC-64 |

`T-63b` dan `T-63c` menjaga keputusan yang bentuknya **ketiadaan aturan**. Keputusan semacam ini
paling mudah dilanggar tanpa sengaja: seorang implementer yang bermaksud baik menambahkan
"peringatan volume terlalu sedikit", dan `RULE-021` hilang tanpa seorang pun memutuskannya.

### 11.5 Waktu penerimaan — `LAB-DEC-042`

| ID | Skenario | Jenis | Yang membuktikan |
|---|---|---|---|
| `T-65a` | Mengirim `ReceivedAt` dari luar **diabaikan**; nilai yang tersimpan tetap dari server | Integrasi | AC-65 |
| `T-65b` | Menelusuri seluruh DTO Laboratorium: tidak ada ruas permintaan bernama `ReceivedAt` | Unit, refleksi | AC-65 |
| `T-66a` | Waktu penerimaan fisik di masa depan ditolak `422` `VAL-58` | Unit | AC-66 |
| `T-66b` | Waktu penerimaan fisik yang mendahului waktu pengambilan ditolak `422` `VAL-59` | Unit | AC-66 |
| `T-67a` | Wadah yang diterima pukul 21.10 dan dicatat pukul 08.05 keesokan hari muncul pada laporan penerimaan **hari pertama**, bukan hari kedua | Integrasi | AC-67 |
| `T-67b` | Selisih waktu nyata terhadap waktu sistem dapat dibaca kepala instalasi | Integrasi | AC-67 |
| `T-17a` | Perhitungan keterlambatan cito **tidak berubah** — tetap dari sampel `Accepted` sampai hasil `Released` | Integrasi | AC-17, BR-37 butir 6 |

`T-67a` adalah skenario yang menjadi alasan `LAB-DEC-042` dibuat. Bila pengujian ini lulus
dengan menempatkan wadah itu pada hari kedua, keputusannya tidak terpenuhi walaupun kolomnya ada.

`T-17a` adalah penjaga regresi: kolom waktu baru **tidak boleh** diam-diam menjadi dasar
perhitungan keterlambatan cito.

### 11.5b Pemesanan per disiplin — `LAB-DEC-055` .. `LAB-DEC-057` (`draft`, 2026-09-15)

| ID | Skenario | Jenis | Yang membuktikan |
|---|---|---|---|
| `T-86a` | Memilih Hemoglobin (PK) dan kultur darah (Mikrobiologi) sekaligus menghasilkan **dua pesanan**, masing-masing berdisiplin tunggal | Integrasi | AC-86 |
| `T-86b` | Kedua pesanan itu **muncul pada menu Pemeriksaan-nya masing-masing**, bukan salah satu saja | Integrasi | AC-86, AC-84 |
| `T-86c` | Memilih tiga pemeriksaan yang seluruhnya Patologi Klinik menghasilkan **satu** pesanan, bukan tiga | Integrasi | AC-86 |
| `T-87a` | Pemeriksaan yang `LabDiscipline`-nya kosong berkumpul menjadi **satu** pesanan tanpa disiplin, dan pemeriksaan lain tetap terpesan | Integrasi | AC-87, `AC-85` |
| `T-88a` | `POST /lab-orders` dipanggil dengan muatan lama **tetap mengembalikan tepat satu pesanan** dengan bentuk respons yang sama | Integrasi, regresi | AC-88 |
| `T-91a` | Setiap baris terpesan dapat ditelusuri ke pemeriksaan yang memenuhinya; yang belum berwadah terbaca sebagai menunggu | Integrasi | AC-91 |
| `T-64a2` | Daftar pemeriksaan kosong ditolak `422` `VAL-64` | Unit | `VAL-64` |
| `T-65a2` | Satu jenis pemeriksaan dipilih dua kali ditolak `422` `VAL-65` | Unit | `VAL-65` |
| `T-66a2` | Pemeriksaan nonaktif atau bukan laboratorium ditolak `422` `VAL-66` | Integrasi | `VAL-66` |
| `T-67a2` | Kunjungan yang sudah ditutup ditolak `422` `VAL-67` | Integrasi | `VAL-67` |
| `T-68a` | Wadah yang memuat pemeriksaan di luar daftar terpesan ditolak `422` `VAL-68` | Integrasi | `VAL-68` |
| `T-69a` | Pemeriksaan terpesan yang sudah berwadah, dimasukkan lagi ke wadah lain, ditolak `409` `VAL-69` | Integrasi | `VAL-69` |
| `T-68b` | **Pesanan lama tanpa baris terpesan tetap menerima wadah apa pun seperti sebelumnya** — `VAL-68` dan `VAL-69` tidak menyentuhnya | Integrasi, regresi | 12.5 arsitektur backend |
| `T-94a` | Konfirmasi pertama berhasil: status `Requested` → `Confirmed`, konfirmator dan waktu terekam dari server | Integrasi | AC-94 |
| `T-94b` | Konfirmasi kedua atas pesanan yang sama ditolak `409` `VAL-70` | Integrasi | AC-94, `VAL-70` |
| `T-94c` | Konfirmasi atas pesanan berstatus `Accepted` atau `InProcess` ditolak `409` `VAL-71` | Integrasi | AC-94, `VAL-71` |
| `T-95a` | Konfirmasi tanpa dokter pemeriksa ditolak `422` `VAL-72`; dokter yang tidak aktif ditolak `422` `VAL-73` | Integrasi | AC-95, `VAL-72`, `VAL-73` |
| `T-95b` | Konfirmator dan waktu konfirmasi **tidak dapat dikirim pemanggil** — ruasnya tidak ada pada DTO permintaan | Unit | AC-95 |
| `T-96a` | Pembatalan tanpa alasan ditolak `422` `VAL-74`; alasan yang diterima terbaca kembali sebagai `ReasonNote` pada jejak audit pesanan itu | Integrasi | AC-96, `VAL-74` |
| `T-97a` | Pembatalan atas pesanan `Accepted`, `InProcess`, `Completed`, atau `Cancelled` ditolak `409` `VAL-75` | Integrasi | AC-97, `VAL-75` |
| `T-97b` | **Pesanan `Requested` dan `Confirmed` tetap dapat dibatalkan seperti sebelumnya** — pengetatan `VAL-75` tidak menutup jalur yang sah | Integrasi, regresi | AC-97 |
| `T-97c` | **Jalur `Requested` → `Accepted` tetap berjalan tanpa konfirmasi** — `LAB-STATE-v1` `r3` tidak mewajibkan `Confirmed` | Integrasi, regresi | `LAB-STATE-v1` `r3` bagian 1a |
| `T-90a` | Satu pemeriksaan ditolak berarti **nol pesanan terbentuk** — transaksinya utuh | Integrasi | 12.3 arsitektur backend |
| `T-83a` | `POST /lab-orders` dipanggil **tanpa ruas disiplin** atas prosedur yang sudah digolongkan: pesanan tersimpan **berdisiplin**, dan muncul pada menu Pemeriksaan yang sesuai | Integrasi | AC-83 |
| `T-83b` | `POST /lab-orders` atas prosedur yang **belum digolongkan**: pesanan tetap tersimpan **tanpa disiplin**, dan itu sah | Integrasi | AC-83, `AC-85` |
| `T-83c` | `POST /lab-orders` yang **membawa** disiplin: nilainya dihormati apa adanya, tidak ditimpa hasil penurunan | Integrasi | AC-83 |
| `T-83d` | **Nol permintaan yang sebelumnya berhasil menjadi gagal** — penurunan disiplin tidak menambah satu pun penolakan baru | Integrasi, regresi | AC-83 |

> **`AC-83` semula tidak punya satu pun baris uji.** Ditemukan pada gate `BE-LAB-29`, 2026-09-15,
> dan ditutup di sini. Kelalaiannya berarti: janji *"tidak ada jalan menyimpan pesanan berdisiplin
> kosong"* tercatat sejak revision 27 tanpa pernah ada cara memeriksanya — dan memang tidak
> pernah diperiksa, sampai pembacaan database menemukan 2 dari 5 pesanan tanpa disiplin.
>
### 11.5c Pendaftaran lewat kiosk — `LAB-DEC-051` .. `LAB-DEC-054`, `LAB-DEC-058`

| ID | Skenario | Jenis | Yang membuktikan |
|---|---|---|---|
| `T-93a` | **Ke-16 sesi kiosk yang sudah tersimpan tetap terbaca** sesudah kedua ruas baru ditambahkan, dengan kedua ruas itu kosong | Integrasi | AC-93 |
| `T-93b` | Alur kiosk yang **tidak** menyebut tujuan layanan berperilaku persis seperti sebelumnya — nol penolakan baru | Integrasi, regresi | AC-93 |
| `T-93c` | Sesi bertujuan Laboratorium yang belum dipakai registrasi muncul pada jalur baca; yang sudah dipakai **tidak** muncul | Integrasi | AC-93 |
| `T-93d` | Sesi bertujuan **selain** Laboratorium tidak ikut muncul ketika penyaring tujuan dipakai | Integrasi | AC-93 |
| `T-92a` | Pasien memilih Laboratorium di kiosk lalu tidak pernah sampai ke meja lab: kunjungannya **tertutup saat hari layanan berakhir** dengan sebab "tidak dilanjutkan" | Integrasi | AC-92 |
| `T-92b` | **Biaya pendaftaran gugur** bersama kunjungan yang tertutup itu | Integrasi | AC-92, `LAB-DEC-058` |
| `T-92c` | Pasien yang **dilanjutkan** petugas **tidak** ikut tertutup | Integrasi, regresi | AC-92 |
| `T-92d` | Pasien yang batal **tidak menghasilkan satu pun tagihan pemeriksaan laboratorium** | Integrasi | AC-90, `AC-37` |

`T-92c` adalah penjaga yang paling penting pada kelompok ini. Penutupan otomatis yang terlalu
rakus tidak muncul sebagai galat — ia muncul sebagai pasien yang pendaftarannya hilang saat ia
sedang duduk menunggu dipanggil, dan yang pertama mengetahuinya adalah pasien itu.

`T-93a` dan `T-93b` menguji **ketiadaan perubahan** pada tabel milik modul lain yang sudah
berisi data nyata. Keduanya syarat yang membuat klaim "aditif" pada `LAB-REQ-006` dapat
dipercaya.

> **`AC-92` dan `AC-93` semula tidak punya satu pun baris uji.** Ditemukan pada gate `BE-EXT-04`,
> 2026-09-15 — pola yang sama persis dengan `AC-83` sehari sebelumnya. Dua kejadian berturut-turut
> menunjukkan ini bukan kelalaian sekali, melainkan celah proses: acceptance criteria yang ditulis
> pada decision log tidak otomatis memperoleh baris uji, dan tidak ada langkah yang memeriksanya.

---

> `T-83d` adalah baris yang paling penting di antara keempatnya. Penurunan disiplin memperluas
> apa yang **berhasil**, bukan memperketat apa yang ditolak; begitu ia mulai menolak sesuatu, ia
> sudah berubah menjadi hal lain.

`T-88a` dan `T-68b` adalah dua pengujian yang paling mudah lupa ditulis, dan keduanya menguji
**ketiadaan perubahan**. `BE-LAB-21` baru saja membuktikan berapa mahal harga endpoint berjalan
yang diam-diam menjadi lebih ketat: layar wadah menjawab `422` sejak migrationnya diterapkan.
Tanpa kedua pengujian ini, klaim "aditif" pada `r10` tidak pernah benar-benar diperiksa.

`T-90a` menjaga janji transaksi. Pemesanan yang gagal separuh akan meninggalkan pasien dengan
satu disiplin terpesan dan satu disiplin hilang — dan hilangnya tidak terlihat sampai hasil yang
ditunggu tidak pernah keluar.

### 11.6 Menu dan layar — `LAB-DEC-045`

| ID | Skenario | Jenis | Yang membuktikan |
|---|---|---|---|
| `T-75a` | Satu pasien rujukan luar dapat diselesaikan dari identifikasi sampai penetapan kelayakan tanpa berpindah menu | Frontend, ujung ke ujung | AC-75 |
| `T-76a` | Seluruh pengujian `lab-orders` dan layar specimen per pesanan yang sudah ada **tetap lulus tanpa perubahan** | Frontend, regresi | AC-76 |
| `T-77a` | Daftar pemeriksaan dan wadahnya terlihat bersamaan sebelum kelayakan ditetapkan | Frontend | AC-77, `LAB-FE-010` |
| `T-77b` | Peringatan penguncian terlihat sebelum tombol kelayakan dapat ditekan | Frontend | `LAB-FE-010` |

`T-76a` adalah syarat yang membuat `LAB-DEC-045` dapat dipercaya. Keputusan itu menjanjikan
layar lama tidak berubah perilakunya; janji itu hanya bermakna bila pengujiannya dijalankan
kembali dan lulus **tanpa disentuh**.

### 11.7 Pengujian migration

| ID | Skenario | Yang membuktikan |
|---|---|---|
| `T-M1` | Ketiga migration berjalan berurutan pada basis data yang sudah berisi `LabSpecimen` | Rencana migration 11.8 |
| `T-M2` | Baris `LabSpecimen` lama tetap ada, tetap terbaca, dan kelima kolom barunya kosong | AC-61 |
| `T-M3` | Unique parsial `IX_LabSpecimenType_SingleOtherBucket` menolak baris `Lainnya` aktif kedua **di tingkat basis data** | `VAL-62` |
| `T-M4` | Menghapus jenis specimen yang sudah dipakai wadah ditolak `DeleteBehavior.Restrict` | 12.1 |

`T-M3` menguji penjagaan yang **juga** ada di service. Keduanya sengaja: aturan yang hanya
dijaga service akan bocor lewat seeder, skrip perbaikan data, atau migration berikutnya.

### 11.8 Yang tidak diuji pada amandemen ini

| Yang tidak diuji | Kenapa |
|---|---|
| `AC-68` sampai `AC-71` — pengusulan instansi perujuk | Terblokir `LAB-COORD-006`; endpointnya belum ada bentuknya |
| `AC-72` sampai `AC-74` — metode pembayaran | Terblokir `LAB-COORD-007`; sumber datanya belum ditetapkan |
| `AC-78` sampai `AC-82` — perlakuan `PaymentType` per jalur dan nilai enum baru | Menunggu jawaban pemilik `registration-management` |
| Pengisian lima baris `MstMeasurement` | Pekerjaan Master Data. Laboratorium menguji bahwa satuan non-laboratorium ditolak (`T-62c`), bukan bahwa barisnya ada |
