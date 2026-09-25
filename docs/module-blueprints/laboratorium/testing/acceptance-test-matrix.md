# Acceptance Test Matrix — Modul Laboratorium

| Field | Value |
|---|---|
| Blueprint ID | `LAB-BP-001` |
| Revision | `8` — amandemen 2026-09-25 kedua (`S4d-1`). Sebelumnya `7` — amandemen 2026-09-25 (`S4`). Sebelumnya `6` — amandemen 2026-09-24 |
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

---

## Amandemen 2026-09-21 — Strategi pengujian `S4b`

Menurunkan decision log rev 50 dan `LAB-API-v1` `r26`. Mencakup `AC-156` sampai `AC-176`.

### Pemetaan acceptance criteria ke lapis pengujian

| AC | Yang diuji | Lapis | Catatan |
|---|---|---|---|
| `AC-156` | Dua pemeriksaan dalam satu order punya dua tempat hasil terpisah | Integrasi | Isi hasil A, baca hasil B, pastikan nol ruas B berubah |
| `AC-157` | `effectiveAt` dan `issuedAt` tidak dapat diketik | Kontrak + integrasi | Kirim keduanya pada request; pastikan **diabaikan**, bukan diterima |
| `AC-158` | `finalize` mengisi `FinalizedAt`, dan hasil Final tetap ditolak untuk dikirim | Integrasi | Uji penolakan pengirimannya, bukan hanya pengisian kolomnya |
| `AC-159` | `reopen` mengosongkan `FinalizedAt` dan bukan koreksi hasil terrilis | Integrasi | Pastikan nol baris `S6` tercipta |
| `AC-160` | Satu specimen menunjuk lebih dari satu Spesifik Specimen | Integrasi | Termasuk penolakan nilai yang diketik bebas |
| `AC-161` | `Lainnya` tidak membuat nilai tetap baru | Integrasi | Pastikan nol baris `LabSpecimenDetailType` bertambah |
| `AC-162` | `2 swab` tersimpan sebagai angka dan satuan | Unit + integrasi | Uji penjumlahan lintas baris bersatuan sama |
| `AC-163` | Baris kepekaan tanpa MIC dan tanpa zona tetap tersimpan | Integrasi | `VAL-101` tidak boleh menolaknya |
| `AC-164` | Kultur tanpa isolat tersimpan tanpa penolakan | Integrasi | **Kasus uji terpenting slice ini** |
| `AC-165` | Baris kepekaan tanpa interpretasi ditolak beserta sebabnya | Integrasi | `VAL-103` |
| `AC-166` | Penanda kritis tidak menyala ketika aturan kosong, **dan layar menyatakannya** | Integrasi + UI | Uji `criticalRuleAvailable`, bukan hanya ketiadaan penanda |
| `AC-167` | `R` yang tidak cocok aturan tidak menyalakan penanda | Integrasi | Lawan dari `AC-166` |
| `AC-168` | `Analis` tidak dapat dipilih dan sama dengan penyimpan | Kontrak + UI | Kirim `analystUserId` palsu; pastikan diabaikan |
| `AC-169` | `Definitif` menyimpan tiga fakta dan tidak membuka tombol apa pun | Integrasi + UI | Pastikan nol tombol pengiriman berubah keadaan |
| `AC-170` | Koreksi specimen berjejak; sesudah Final menjadi baca-saja | Integrasi | `VAL-109` |
| `AC-171` | Dokter Konfirmator menawarkan DPJP dan dokter bertugas, nol peran baru | Integrasi | Periksa matriks permission tidak bertambah peran |
| `AC-172` | Nol ruas HL7 dalam bentuk apa pun | Kontrak + UI | Termasuk memastikan tiada pilihan kosong yang tak dapat dipilih |
| `AC-173` | Ketika jadwal jaga terisi, hanya dokter itu yang ditawarkan | Integrasi | Butuh data uji `TrxOnCallAssignment` |
| `AC-174` | Ketika jadwal jaga kosong, pemilih tetap dapat dipakai | Integrasi | **Keadaan yang pasti terjadi lebih dulu** — `LAB-COORD-014` |
| `AC-175` | Koreksi menambah satu baris jejak ruas dan **nol** baris jejak status | Integrasi | Menguji pemisahan `LAB-DEC-112` secara langsung |
| `AC-176` | Status temuan menawarkan tepat tiga nilai | Kontrak + UI | Pastikan `NeedsAttention` dan `Critical` **tidak** muncul |

### Tiga kasus uji yang paling mudah terlewat

| Kasus | Kenapa mudah terlewat | Kenapa penting |
|---|---|---|
| `AC-164` kultur steril | Pengujian cenderung memakai data yang "lengkap" | Hasil negatif adalah hasil yang **paling sering** keluar dari kultur, dan `VAL-88` pernah dicabut justru karena kesalahan kelas ini |
| `AC-166` aturan kritis kosong | Layar bersih terlihat seperti lulus | Layar bersih justru **keadaan berbahaya**: petugas menyimpulkan hasil aman padahal aturannya belum ada |
| `AC-174` jadwal jaga kosong | Dianggap keadaan sementara | Ia keadaan **awal** dan mungkin bertahan lama — `TrxOnCallAssignment` nol punya endpoint pengisi |

### Data uji yang harus disiapkan

| Data | Kenapa |
|---|---|
| Satu order berisi **dua** pemeriksaan Mikrobiologi | `AC-156` tidak dapat diuji dengan satu pemeriksaan |
| Satu `LabMicrobiologyCriticalRule` beserta keadaan **nol baris** | `AC-166` dan `AC-167` menguji dua keadaan berlawanan |
| Satu `TrxOnCallAssignment` aktif beserta keadaan **nol baris** | `AC-173` dan `AC-174` |
| Satu `LabSpecimenType` bertanda `IsOtherBucket` | `VAL-104` |

### Yang tidak diuji pada slice ini

Validasi dan rilis (`S4d`), pengiriman hasil (`LAB-COORD-011`), cetak dwibahasa
(`LAB-COORD-013`), dan tata letak cetak (`LAB-OPEN-039`). Keempatnya di luar `S4b`.

---

## Amandemen 2026-09-21 (kedua) — `AC-177`..`AC-191`

| AC | Lapis | Catatan pengujian |
|---|---|---|
| `AC-177` | Integrasi | Kualifikasi tersimpan sebagai nilai; **bukan** disimpulkan dari catatan konsultasi |
| `AC-178` | Integrasi | Kirim kadar tanpa satuan → `422` (`VAL-112`) |
| `AC-179`, `AC-188` | Kontrak + UI | Uji **empat kombinasi** jenis biakan × metode uji, termasuk **jamur + difusi cakram** |
| `AC-180` | Integrasi | Dua nomor berbeda pada satu pesanan, keduanya terbaca |
| `AC-181` | Integrasi | Ambil bahan hari Senin, terima hari Rabu → cetakan menulis Rabu, layar menulis Senin |
| `AC-182` | Integrasi | Ubah nama konsultan → footer berubah, pemegang wewenang klinis **tidak** |
| `AC-183` | Integrasi + UI | Sebelum rilis, ruas kosong — **bukan** nama pencetak maupun penulis hasil |
| `AC-185` | Integrasi | Ubah breakpoint di data induk → baris hasil lama **tetap** memakai snapshot lamanya |
| `AC-186` | Unit + integrasi | **Kasus uji terpenting.** Ketiga batas: zona 11 pada 12-16 → `R`; 13 pada 12-15 → `I`; 32 pada 13-16 → `S` |
| `AC-187` | Integrasi | Timpa tanpa alasan → `422` (`VAL-113`); dengan alasan → tersimpan beserta `ComputedResult` aslinya |
| `AC-189` | Integrasi + UI | Pemeriksaan tanpa profil set bakteri → bagian isolat **nol tampil**, dan mengirimnya → `422` (`VAL-118`) |
| `AC-190` | Integrasi | Isolat tanpa baris kepekaan tersimpan; bertanda tidak diuji **tetapi** punya baris → `422` (`VAL-117`) |
| `AC-191` | Integrasi | Zona `0` → `R`; zona dikosongkan → **nol** interpretasi dihitung dan `result` menjadi wajib |

### Data uji tambahan

Satu `LabSusceptibilityBreakpoint` **beserta keadaan nol baris** — `AC-186` dan jalur manual
`VAL-114` menguji dua keadaan berlawanan, dan keadaan **nol baris** adalah yang pasti terjadi
lebih dulu.

---

## Amandemen 2026-09-24 — Perluasan hasil Patologi Klinik dan perbaikan `S4b`

| Field | Nilai |
|---|---|
| Status | **`draft`** |
| Kontrak yang diuji | `LAB-API-v1` `r33`, `LAB-VAL-v1` `r11`, `LAB-PERM-v1` revision 10, `LAB-STATE-v1` `r4` — seluruhnya **`approved` 2026-09-24** |
| Rancangan | `02-backend-architecture.md` bagian 19; `03-frontend-architecture.md` amandemen 2026-09-24 |

### Matriks

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `AC-221` | Pengguna yang hanya memegang `LabExamination : Update` memanggil `PUT /result`, `PUT /result/microbiology`, `POST /result/finalize`, `POST /result/reopen`, `PUT /result/consultation` | Integrasi hak akses | **Kelimanya `403`**; `PUT /urgency` oleh pengguna yang sama tetap `200` |
| `AC-222` | Pengguna yang hanya memegang `LabExaminationResult : Update` mengisi hasil, lalu mencoba batal, cito, duplo | Integrasi hak akses | Isi hasil `200`; batal, cito, duplo `403` |
| `AC-223` | Sesudah deploy dan langkah 19.7, analis yang kemarin mengisi hasil mengisi lagi tanpa campur tangan admin tambahan | Uji rilis manual | Satu hasil tersimpan dalam jendela rilis |
| `AC-224` | Laporan Patologi Anatomi diisi, Final, dan Reopen oleh pemegang izinnya sekarang | Regresi | Ketiganya berjalan seperti sebelum amandemen |
| `AC-225` | Hasil Patologi Klinik dan Mikrobiologi yang sudah Final disimpan ulang | Integrasi | `409` `VAL-120`; isi di basis data **tidak berubah** |
| `AC-225` — jalur gagal setengah jalan | Hasil Mikrobiologi Final dengan satu isolat dan dua belas baris antibiogram, lalu disimpan ulang dengan isolat pengganti | Integrasi | `409`; **tetap** satu isolat dan dua belas baris — nol penghapusan sempat terjadi |
| `AC-226` | Konsultasi dicatat pada hasil Final | Integrasi | `409` `VAL-121` |
| `AC-227` | Reopen beralasan, simpan, Final lagi | Integrasi | `ReopenCount` naik satu; satu baris `LabTransitionHistory` beralasan; `FinalizedAt` baru |
| `AC-228` | Halaman Mikrobiologi menerima `409` | Unit test aturan + UI | Pesan terbaca; nilai yang diketik tetap di isian |
| `AC-196` | Hasil Patologi Klinik Draft | Integrasi | Tidak dapat Reopen (`VAL-107`); belum masuk antrean validasi — **antrean diuji bersama `S4`** |
| `AC-197` | Reopen hasil Patologi Klinik yang Final | Integrasi | `FinalizedAt` kosong, riwayat mencatat pelaku dan waktu |
| `AC-214` | Hasil Patologi Klinik Final tanpa konsultasi; lalu hasil lain dengan konsultasi | Integrasi | Keduanya sah; ketiga fakta konsultasi terbaca; nol pilihan `Definitif` |
| `AC-219` | Kalium 6,4 pada rujukan 3,5-5,1; Hemoglobin 9,4 pada 13,0-17,0; Protein urin `+2` di luar rujukan | Kontrak + UI | `High` → `H 6,4`; `Low` → `L 9,4`; `OutOfReference` → teks — **cetak hitam-putih tetap terbaca** |
| `AC-234` | Buka order Patologi Klinik berisi 19 pemeriksaan, satu dibatalkan | Integrasi + UI | Satu panggilan `GET /by-order/{id}/results` mengembalikan **18** baris |
| `AC-234` — jalur gagal | Buka order Mikrobiologi lewat jalur yang sama | Integrasi | `422` `VAL-123` |
| `AC-235` | Buka Daftar Kerja | UI | Nol dialog isi hasil; aksi baris membuka halaman order |
| `AC-236` | Final Kalium cito, Hemoglobin masih diisi | Integrasi + UI | Kalium Final; Hemoglobin tetap dapat disimpan |
| `AC-237` | Isi dua baris, simpan satu | Unit test aturan | Isian baris kedua tetap ada |
| `VAL-122` | Final atas baris pemeriksaan Patologi Anatomi | Integrasi | `422` dengan pesan yang mengarahkan ke laporan PA |

### Yang tidak diuji pada amandemen ini

| Yang tidak diuji | Alasan |
|---|---|
| Antrean validasi, rilis, *Kembalikan ke analis*, kewenangan dari kredensial Human Resource (`AC-229`..`AC-233`) | `S4` dihentikan — `DEC-LAB-011` |
| Label order *Dalam Pemeriksaan*/*Selesai* (`AC-198`, `AC-199`) | Bergantung rilis |
| Penanda `KRITIS` | `S5` |

---

## Amandemen 2026-09-25 — Validasi dan rilis hasil Patologi Klinik (`S4`)

| Field | Nilai |
|---|---|
| Status | **`draft`** |
| Kontrak yang diuji | `LAB-API-v1` `r34`, `LAB-VAL-v1` `r12`, `LAB-PERM-v1` revision 11, `LAB-STATE-v1` `r5`, `LAB-INT-v1` `r4` — **seluruhnya `approved` 2026-09-25** |
| Rancangan | `02-backend-architecture.md` bagian 20; `03-frontend-architecture.md` amandemen 2026-09-25; kamus data bagian 17 |
| Kesiapan | Desain saja — pemakaian nyata tertahan `DEC-LAB-011` sisa, `DEC-LAB-017`, `DEC-LAB-018`, `LAB-COORD-016` |

### Matriks — tindakan dan keadaan hasil

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `AC-196` | Kalium Draft; tahap antrean `AwaitingValidation`; lalu validasi Kalium | Integrasi | Kalium **tidak** ada di antrean; validasi `422` `VAL-124` |
| `AC-197` | Reopen Kalium Final yang belum divalidasi; ulangi sesudah divalidasi | Integrasi | Pertama `200`; kedua `409` `VAL-136`, `FinalizedAt` tetap terisi |
| `AC-01` | Pengguna yang mengisi Kalium sekaligus memegang aksi dan kode validasi menekan Validasi tanpa alasan; lalu dengan alasan | Integrasi | Pertama `422` `VAL-129`; kedua `200`, `ValidationExceptionReasonId` dan salinan namanya terisi |
| `AC-02` | Buka hasil dari `AC-01` | Integrasi + UI | `validationExceptionMarker` berbunyi persis usulan 20.10 butir 5; tampil di halaman hasil; baris riwayat `ValidateResult` membawa kode alasan. **Cetakan menyusul `S17`** |
| `VAL-131` | Pemvalidasi yang memegang kode rilis merilis hasilnya sendiri tanpa alasan; lalu dengan alasan | Integrasi | Pertama `422`; kedua `200` dan `releaseExceptionMarker` berbunyi *"Dirilis oleh pemvalidasi sendiri — …"* |
| `VAL-132` | (a) Alasan pengecualian dikirim padahal pelaku tidak merangkap; (b) alasan nonaktif; (c) alasan `requiresNote` tanpa catatan | Integrasi | Ketiganya `422`; **nol** kolom berubah |
| `VAL-130` | Validasi hasil yang `ResultEnteredByUserId`-nya kosong | Integrasi | `422` dengan pesan yang menyuruh analis menyimpan ulang |
| `VAL-126` | Validasi, rilis, dan *Kembalikan* atas pemeriksaan Mikrobiologi dan Patologi Anatomi | Integrasi | Keenamnya `422` |
| `VAL-127` | Validasi pemeriksaan `Cancelled`, `Voided`, dan pemeriksaan pada order `Cancelled` | Integrasi | Ketiganya `422` |
| `VAL-125`, `VAL-133` | Validasi dua kali; rilis dua kali | Integrasi | Kedua yang kedua `409`; **tetap satu** baris riwayat per tindakan |
| `AC-205` | *Kembalikan* tanpa alasan; lalu dengan alasan *Sampel tertukar* | Integrasi | Pertama `422` `VAL-135`; kedua `200`, hasil **Draft**, `FinalizedAt` dan kolom validasi kosong, `ReopenCount` **tidak** naik |
| `AC-206` | Baca riwayat hasil dari `AC-205` | Integrasi | Baris `ValidateResult` (pelaku, waktu) **masih ada dan tidak berubah**; baris `ReturnResultToAnalyst` berkode alasan; **nol** pemberitahuan, **nol** versi bernomor |
| `AC-207` | *Kembalikan* hasil yang sudah dirilis | Integrasi | `409` `VAL-134` |
| `VAL-143` | Batal pemeriksaan yang sudah dirilis; lalu batal pemeriksaan yang tervalidasi tetapi belum dirilis | Integrasi | Pertama `422`; kedua **tetap berjalan** seperti hari ini |
| `AC-08`, `AC-198` | Order berisi Kalium cito dan Hemoglobin; Kalium dirilis, Hemoglobin masih Draft | Integrasi | Kalium `Released`; `resultProgress` order = `InProgress` |
| `AC-199` | Semua pemeriksaan tidak batal dirilis; satu pemeriksaan lain dibatalkan | Integrasi | `resultProgress` = `AllReleased`; pemeriksaan batal tidak menahannya; **nol kolom** baru pada `LabOrder` — dibuktikan pada migration |

### Matriks — kewenangan dua lapis

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `AC-229` | Dokter berjabatan calon dengan kode validasi PK aktif dalam masa berlaku | Integrasi | `200`; `ValidatedByPrivilegeId` menunjuk baris penunjukan itu |
| `AC-215` | Jabatan calon, **nol** baris penunjukan | Integrasi | `403` *"Anda belum ditunjuk…"* |
| `AC-216` | Punya baris penunjukan, jabatannya **tidak** memegang `Validate` | Integrasi hak akses | `403` dari filter, sebelum service |
| `AC-217`, `AC-231` | Hanya kode validasi → rilis; hanya kode rilis → validasi | Integrasi | Keduanya `403` |
| `AC-230` | Penunjukan `Suspended`, `Revoked`, `Expired`, dan di luar masa berlaku | Integrasi | Keempatnya `403`; `Version` dan seluruh kolom `LabExamination` **tidak berubah** |
| `AC-233` | Delapan sebab `LabPrivilegeDenial`, termasuk akun tanpa `WorkforceProfileId` dan `IsClinicalServiceBlocked` | Integrasi | Delapan pesan berbeda, masing-masing menyebut sebabnya (`LAB-VAL-v1` `r12` 14.2) |
| `VAL-128` — tanggal inklusif | Penunjukan berakhir 30 September; validasi 23.50 WIB tanggal 30, lalu 00.05 WIB tanggal 1 | Unit + integrasi | Pertama diterima; kedua ditolak *"sudah habis pada 30 September 2026"* |
| `VAL-128` — pembacaan gagal | Resolver dipaksa gagal membaca | Unit | `503`; **nol** perubahan tersimpan |
| `VAL-128` — kode belum ada di katalog | Katalog Human Resource belum memuat kode `LabClinicalPrivilegeCodes` | Integrasi | Setiap validasi `403` `NotAppointed` — **fail-closed** |
| `AC-232` | Jalankan seluruh skenario di atas | Integrasi + tinjauan kode | Jumlah dan `UpdateDateTime` baris `WfpClinicalPrivilege` **sama** sebelum dan sesudah; nol `Add`/`Update` entity Human Resource di Laboratorium |
| `AC-238` | Analis yang namanya pernah ditunjuk memvalidasi; dokter yang ditunjuk memvalidasi | Integrasi hak akses | Analis `403`; dokter `200` |
| `AC-239` | Validasi; lalu jabatan dokter dinamai ulang dan ia dipindah jabatan | Integrasi | `validatedByPositionName` hasil lama **tidak berubah** |
| `AC-239` — dua penempatan | Dokter punya dua penempatan aktif; hanya satu yang jabatannya memegang `Validate` | Unit | Snapshot menunjuk penempatan **yang memberi izin**, bukan penempatan utama yang tidak memegangnya |

### Matriks — integrasi, konkurensi, antrean, data induk

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `INT-08` | Rilis satu hasil | Integrasi | **Tepat satu** baris `MrcClinicalDocumentIntegrity` berjenis `LaboratoryResult`, `DocumentId` = id pemeriksaan, `Signed`, `LockedAt` = waktu rilis |
| `INT-08` — gagal | Rilis hasil yang kunjungannya tidak sah (data uji) | Integrasi | `422` `VAL-137`; **nol** `ReleasedAt`, **nol** baris riwayat `ReleaseResult`, **nol** baris rekam medis; hasil tetap `Validated` |
| `INT-08` — kunjungan tertutup | Rilis hasil pasien rawat jalan yang kunjungannya sudah ditutup | Integrasi | `200`; dokumen tertanda tangan dan terkunci |
| Konkurensi — Reopen lawan Validasi | Keduanya dikirim bersamaan pada Kalium Final | Integrasi berpasangan | Satu `200`, satu `409`. **Tidak pernah** ada baris dengan `ValidatedAt` terisi dan `FinalizedAt` kosong |
| Konkurensi — dua validasi | Dua dokter memvalidasi Kalium yang sama bersamaan | Integrasi berpasangan | Satu `200`, satu `409`; satu baris riwayat `ValidateResult` |
| Antrean | Tahap `AwaitingValidation` dan `AwaitingRelease`; tanpa `stage` | Integrasi | Isi tiap tahap sesuai 29.4; cito di atas; tanpa `stage` → `422` `VAL-139` |
| `AC-17` | Kalium cito yang sudah terlambat lalu dirilis | Integrasi | Kalium **hilang** dari `GET /cito-overdue` dan `GET /pending` sesudah rilis |
| `VAL-140`..`VAL-142` | Kode ganda; kode berhuruf kecil; mengubah kode lewat `PUT` | Integrasi | `409`; `422`; `422` |
| Data induk — `requiresNote` | `POST` membawa `requiresNote = true`; lalu `PUT /system-flags` oleh pengguna tanpa `SystemFlag` | Integrasi hak akses | Ruas diabaikan pada `POST`; `403` pada `system-flags` |
| Nol status baru | Bandingkan `LabExaminationStatus` sebelum dan sesudah | Regresi | Tetap empat nilai (`LAB-DEC-080`) |

### Matriks — layar

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `LAB-FE-004` | Hasil berpengecualian dibuka di halaman hasil per order | UI | Penanda **terbaca sebagai teks**, bukan warna saja |
| Peringatan pengisi | Pengisi hasil membuka barisnya sendiri dan menekan Validasi | Unit test aturan + UI | Pilihan alasan pengecualian muncul **sebelum** permintaan dikirim |
| `403` lapis orang | Dokter yang belum ditunjuk menekan Validasi | UI | Pesan sebab dari backend tampil pada baris itu; tombol tidak hilang diam-diam |
| Antrean | Buka `lab-worklists/validation-queue` | UI | Dua tahap; baris membuka halaman hasil ordernya |

### Data uji tambahan

| Data | Isi |
|---|---|
| Pengguna | Analis (`LabExaminationResult : Update`); dokter A (`Validate`, `Return`, kode validasi **dan** rilis PK); perilis B (`Release`, `Return`, kode rilis PK); pengguna tanpa `WorkforceProfileId` |
| Penunjukan Human Resource | Satu baris per keadaan: `Active`, `Pending`, `Suspended`, `Revoked`, `Expired`, belum berlaku, berakhir hari ini, `IsClinicalServiceBlocked` |
| Daftar alasan | Pada kedua daftar: satu aktif, satu nonaktif, satu `requiresNote` |
| Order | Patologi Klinik berisi Kalium cito, Hemoglobin, dan satu pemeriksaan yang dibatalkan |

**Kode kewenangan pada data uji** memakai nilai `LabClinicalPrivilegeCodes` yang sedang berlaku —
jangan menulis `LAB-VAL-PK` langsung di test, sebab nilainya baru final lewat `LAB-COORD-016`.

### Yang tidak diuji pada amandemen ini

| Yang tidak diuji | Alasan |
|---|---|
| Baris *Validasi oleh* dan *Otorisasi oleh* pada cetakan | Cetakan Patologi Klinik milik `S17` |
| `AC-218` — pesan menyebut disiplin yang belum ditunjuk | Baru dapat dibuktikan bersama `S4d` |
| `AC-03`, `AC-04` — hasil kritis dan pelaporannya | `S5` |
| Koreksi sesudah rilis | `S6` |
| Peringatan satu pemegang per shift | Ditunda — `02-backend-architecture.md` 20.9 |
| Simpan hasil, Final, dan konsultasi bersamaan | Milik `MVP-8` (`02-backend-architecture.md` 20.12) |

---

## Amandemen 2026-09-25 (kedua) — Validasi dan rilis Mikrobiologi (`S4d-1`)

| Field | Nilai |
|---|---|
| Status | **`draft`** |
| Kontrak yang diuji | `LAB-API-v1` `r35`, `LAB-VAL-v1` `r13`, `LAB-STATE-v1` `r6`, `LAB-INT-v1` `r5` — **`draft`**; `LAB-PERM-v1` revision 11 apa adanya |
| Rancangan | `02-backend-architecture.md` bagian 21; `03-frontend-architecture.md` amandemen 2026-09-25 (kedua) |

**Seluruh baris amandemen 2026-09-25 berlaku juga bagi Mikrobiologi** — validasi, rilis,
pengembalian, empat mata, dua lapis, konkurensi, `INT-08`. Yang di bawah adalah **tambahan**.

### Matriks

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `AC-241` | Pemegang kode validasi **Patologi Klinik** saja memvalidasi kultur urin; lalu dr. Nabila — pemegang kode validasi **Mikrobiologi** — memvalidasinya | Integrasi | Pertama `403` dengan kata *Mikrobiologi* pada pesannya; kedua `200` |
| `VAL-144` | Validasi hasil berkualifikasi `Sementara` | Integrasi | `422`; **nol** kolom berubah |
| `VAL-144` — jalur Reopen | Hasil `Sementara` dibuka kembali, kualifikasi diubah `Definitif`, Final ulang, lalu divalidasi | Integrasi | Validasi `200` |
| `ARCH-GAP-LAB-10` | Validasi hasil dengan kualifikasi **kosong** | Integrasi | `200` — kosong **tidak** dianggap sementara |
| `INV-53` | Sesudah validasi, simpan hasil dengan isolat pengganti; sesudah rilis, sama | Integrasi | Keduanya `409` `VAL-120`; isolat dan antibiogram **tidak berubah** |
| `INV-53` — pengembalian | *Kembalikan ke analis* atas hasil tervalidasi, lalu ubah isolat | Integrasi | Pengembalian `200`; simpan isolat `200`; baris `ValidateResult` lama tetap ada |
| `INT-08` Mikrobiologi | Rilis kultur urin | Integrasi | **Tepat satu** baris `MrcClinicalDocumentIntegrity` `LaboratoryResult` untuk pemeriksaan itu; **nol** baris untuk isolatnya |
| 30.3 — ruas pengesah | Baca `GET /{id}/result/microbiology` sebelum validasi, sesudah validasi, dan sesudah rilis | Integrasi | Sebelum: kedua ruas kosong (`AC-183`). Sesudah validasi: `validatedByName` terisi, `authorizingOfficerName` kosong. Sesudah rilis: keduanya terisi, `isReleased = true` |
| `VAL-126` bunyi baru | Validasi pemeriksaan Patologi Anatomi | Integrasi | `422` *"…Patologi Anatomi belum tersedia."* |
| `VAL-145` | Antrean dengan `discipline = AnatomicalPathology` | Integrasi | `422` |
| Antrean dua disiplin | Antrean tanpa `discipline`; lalu `discipline = Microbiology` | Integrasi | Pertama memuat kedua disiplin; kedua hanya Mikrobiologi; hasil `Sementara` **tidak ada** di keduanya |
| `AC-199` Mikrobiologi | Order dengan kultur urin dirilis dan kultur darah `Sementara` | Integrasi | `resultProgress = InProgress` |
| Layar — `Sementara` | Buka hasil `Sementara` Final di Halaman Hasil Mikrobiologi | Unit test aturan + UI | Tombol Validasi tidak ditawarkan; keterangan terbaca |

### Data uji tambahan

Pengguna: dr. Nabila samaran dengan kode validasi Mikrobiologi; dokter pemegang kode validasi
Patologi Klinik **saja**. Order Mikrobiologi: satu kultur `Definitif`, satu `Sementara`, satu
berkualifikasi kosong.

### Yang tidak diuji

| Yang tidak diuji | Alasan |
|---|---|
| Rilis hasil `Sementara` dan penggantiannya | `S4d-2` — `DEC-LAB-020` |
| Cetakan Mikrobiologi | Belum ada di frontend — `S17` |
| Patologi Anatomi | `S4e` — `DEC-LAB-021` |
