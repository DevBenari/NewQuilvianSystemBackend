# Laporan Perubahan Backend — `BE-RAD-13`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RAD-13` |
| Judul | Daftar kerja per alat |
| Slice | `S12` — Daftar kerja petugas |
| Roadmap | `docs/module-blueprints/radiologi/roadmap/backend-roadmap.md` bagian 5, gelombang `MVP-4` |
| Trace | `FR-RAD-060`, `FR-RAD-061`, `FR-RAD-062`; `RAD-DEC-012`, `RAD-DEC-013`; `RAD-API-001` endpoint `GET /worklist` dan `PUT /{id}/urgency` |
| Contract version | `RAD-API-001` rev 9 dan `RAD-PERM-001` rev 7 saat dikerjakan. Diamandemen menjadi **rev 10** dan **rev 8**, **menunggu konfirmasi pemilik modul** |
| Dependency | `BE-RAD-12` — **selesai** |
| Klasifikasi | `HEAVY` — skor 9: repository 0, berkas diperiksa 1, berkas diubah 1, logika bisnis **2**, kontrak API **2**, database 1, keamanan/auth 1, workflow **1** |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/RadiologyManagement/`, `Tests/QuilvianSystemBackend.UnitTests.InMemory/`, `docs/module-blueprints/radiologi/` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `bac46079` |
| Tanggal | 2026-09-11 |
| Status | **Selesai.** Dua endpoint terakhir roadmap backend berjalan. Build lulus 0 error; 283 uji radiologi lulus, 24 di antaranya baru; 1.488 uji in-memory dan 481 uji Sqlite lulus. **Dengan ini seluruh 15 task backend roadmap Radiologi selesai** |

### Preflight Kontrak Rekayasa Backend

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `RadiologyManagement / Radiology` |
| Pemilik / prefix registry | Prefix `Rad`, lifecycle `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`; `QBE-API-001`; `QBE-PERM-001`; `QBE-DTO-001`; `QBE-VAL-001`; `QBE-LOG-001`; **`QBE-AUD-001`** — perubahan penanda cito masuk audit database (`RadTransitionHistory`), terpisah dari application logging; `QBE-MOD-001`; `QBE-ENT-003` — **tidak ada kolom baru yang dipersistensi** untuk daftar kerja |
| QBE ID yang **tidak** berlaku | `QBE-ENT-001`, `QBE-CFG-001`, `QBE-DB-001`, `QBE-DB-002` — tidak ada entity, configuration, maupun migration; `QBE-CODE-*`; `QBE-PAGE-001` — daftar kerja satu hari satu alat tidak berhalaman, dan kontrak memang tidak memintanya |
| Pengecualian yang disetujui | `NONE` |

---

## 1. Masalah yang diperbaiki

Setelah `BE-RAD-12`, penanda cito tersimpan — tetapi **tidak ada satu pun layar yang dapat
membacanya sebagai daftar kerja**. Petugas radiologi tidak punya cara melihat "apa saja pekerjaan
CT-Scan hari ini", dan penanda cito yang sudah dicatat tidak mendahulukan apa pun.

Dan penanda itu hanya dapat ditetapkan **saat pesanan dibuat**. Keadaan pasien berubah:

> **Contoh nyata.** Pukul 09.00 dokter memesan CT kepala rutin untuk pasien rawat jalan. Pukul
> 11.00 kesadaran pasien menurun di ruang tunggu.
>
> Sampai task ini, tidak ada satu pun jalan untuk menaikkan pesanan itu menjadi cito. Satu-satunya
> cara adalah membatalkan pesanan lama dan membuat pesanan baru — yang berarti kehilangan seluruh
> riwayatnya, dan membuat pasien kehilangan antrean yang sudah ia tunggu dua jam.

---

## 2. Proses bisnis

**Tujuan.** Petugas membuka daftar pekerjaan pada alat tempat ia bertugas, dengan yang mendesak
di urutan atas; dan penanda mendesak dapat diubah ketika keadaan pasien berubah.

### 2.1 Alur daftar kerja

| Langkah | Pelaku | Yang terjadi |
| ---: | --- | --- |
| 1 | Radiografer Tono | Membuka daftar kerja **CT-Scan** untuk hari ini |
| 2 | Sistem | Mengumpulkan pesanan CT-Scan hari itu beserta study yang sudah lahir |
| 3 | Sistem | Menempatkan pesanan cito di urutan atas; sesama cito diurutkan menurut waktu pesanan |
| 4 | Radiografer Tono | Mengerjakan dari atas. Pemeriksaan MRI dan USG hari itu tidak muncul di layarnya |

### 2.2 Tiga hal yang paling menentukan

**Pertama — alat wajib dipilih, dan tanpanya ditolak `400`.**

Di radiologi, penempatan petugas mengikuti **ruang alat**, bukan mengikuti pasien
(`RAD-DEC-012`). Daftar kerja tanpa alat bukan daftar kerja siapa pun — dan mengembalikan seluruh
pekerjaan rumah sakit ketika alatnya tidak disebut berarti memuat ribuan baris yang tidak
seorang pun minta.

**Kedua — pesanan cito di urutan atas, sesama cito menurut waktu.**

> **Contoh berangka, langsung dari `FR-RAD-062`.** Ada empat pesanan CT-Scan rawat jalan dari
> pukul 08.00 sampai 10.00, dan satu pesanan cito dari IGD pukul 10.30. Daftar kerja menampilkan
> pesanan IGD di **urutan pertama**, mendahului keempat pesanan yang lebih tua.
>
> Bila ada **dua** pesanan cito, keduanya tetap di atas — dan di antara keduanya urutannya kembali
> mengikuti waktu: yang lebih dulu dipesan dikerjakan lebih dulu, bukan yang terakhir ditandai.

**Ketiga — hari kerja dihitung menurut Waktu Indonesia Barat, bukan UTC.**

Ini terlihat seperti perkara kerapian sampai seseorang masuk shift pagi:

> Pukul **06.00 WIB** berjalan pada pukul **23.00 UTC hari sebelumnya**. Kalau harinya dihitung
> menurut UTC, petugas yang baru masuk pukul 06.00 akan membuka daftar kerja dan melihat
> **pekerjaan kemarin** — tepat pada jam ketika seluruh pekerjaan hari itu belum satu pun
> terlihat.

Waktu yang menempatkan sebuah pesanan pada sebuah hari adalah **jadwalnya** bila sudah
dijadwalkan, kalau tidak waktu pemesanan, kalau tidak waktu pesanan dibuat. Pesanan yang dibuat
kemarin untuk dikerjakan hari ini adalah milik daftar kerja **hari ini**.

### 2.3 Alur mengubah penanda cito

| Percobaan | Jawaban sistem | Kode |
| --- | --- | --- |
| Menaikkan pesanan berjalan menjadi cito | Berhasil; tercatat siapa dan kapan | `200` |
| Mencabut penanda cito | Berhasil; kolom jejaknya dikosongkan | `200` |
| Menetapkan penanda ke nilai yang sudah berlaku | Berhasil, **tanpa meninggalkan jejak baru** | `200` |
| Mengubah penanda pesanan yang sudah selesai, dibatalkan, atau ditolak | "Pesanan ini berstatus … sehingga penanda citonya tidak dapat diubah lagi." | `409` |
| Mengubah penanda pesanan yang tidak ada | "Pesanan radiologi tidak ditemukan." | `404` |
| Daftar kerja tanpa `modalityId` | "Alat pencitraan wajib dipilih untuk membuka daftar kerja." | `400` |

**Mengapa pesanan yang sudah selesai ditolak.** Pesanan semacam itu tidak lagi mengantre di
daftar kerja mana pun, sehingga mendahulukannya tidak mendahulukan apa pun — yang tersisa hanya
jejak audit yang membingungkan pembacanya kelak. Ketentuan ini **tidak tercantum** pada kontrak
mana pun; ia keputusan pelaksana dan dicatat apa adanya di sini.

**Mengapa menekan tombol yang sama dua kali tidak meninggalkan jejak.** Permintaan yang tidak
mengubah apa pun bukan keputusan baru. Mencatatnya akan mengisi riwayat dengan baris yang
terbaca seolah seseorang berulang kali memutuskan hal yang sama.

### 2.4 Setiap perubahan penanda masuk riwayat, bukan hanya kolom

Kolom `UrgentMarkedByUserId` dan `UrgentMarkedAt` hanya menyimpan keadaan **sekarang**. Ketika
penanda dicabut, keduanya dikosongkan — dan jejak siapa yang pernah memasangnya akan hilang kalau
tidak disimpan di tempat lain.

Karena itu setiap perubahan dicatat pada `RadTransitionHistory` sebagai aksi `Order.Urgency`,
lengkap dengan keadaan sebelum, sesudah, dan pelakunya.

> **Pertanyaan "siapa yang MENCABUT penanda cito pasien ini" sama pentingnya dengan "siapa yang
> memasangnya"** — dan hanya riwayat yang dapat menjawabnya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `AGENTS.md`, `docs/engineering/` beserta registry | Governance canonical dan preflight QBE |
| `rules/backend/transaction-endpoint-standard.md` bagian 2.2 dan 3 | Arketipe worklist dan aturan verb aksi — sumber selisih pada bagian 3.4 |
| `rules/backend/` — `TASK_RULES`, `TASK_CLASSIFICATION`, `REVIEW_RULES`, `REPORT_TEMPLATE` | Aturan operasional task |
| `00-interview-decisions.md` `RAD-DEC-012` beserta kriteria 36–42 | Keputusan asal, termasuk alasan memilih per alat |
| `04-prd-to-mvp.md` `FR-RAD-060` s/d `FR-RAD-062` | Contoh berangka yang menentukan urutan |
| `testing/acceptance-test-matrix.md` bagian 7b | Sembilan baris bukti yang diharapkan |
| `contracts/api-contract.md` bagian *Rad Order — tambahan* | Bentuk kedua endpoint beserta parameter wajibnya |
| `Services/RadOrderService.cs`, `RadStudyService.cs`, `RadOrderController.cs` | Yang disentuh |
| `Services/BillingManagement/.../BillingNumberSeriesService.cs` | Pola penentuan zona waktu bisnis yang sudah dipakai repository ini |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Services/RadOrderService.cs` | `GetWorklistAsync` dan `SetUrgencyAsync`; pemeta `MapWorklist`; penentu rentang hari kerja WIB |
| `Areas/.../Controllers/RadOrderController.cs` | Dua endpoint: `GET /worklist` dan `PUT /{id}/urgency` |
| `Areas/.../DTOs/RadiologyDtos.cs` | `RadOrderUrgencyRequest`, `RadWorklistItemResponse`, `RadWorklistStudyResponse` |
| `Areas/.../Services/RadStudyService.cs` | `LabelStatusStudy` dari `private` menjadi `internal` |
| `Areas/.../Services/RadOperationResult.cs` | Dua kode galat baru |
| `Tests/.../RadWorklistTests.cs` | **Baru.** 24 uji |

### 3.3 Label keadaan study dipakai ulang, bukan disalin

`RadStudyService.LabelStatusStudy` diubah dari `private` menjadi `internal` supaya daftar kerja
memakai peta label yang sama persis dengan layar study.

Menyalinnya akan membuat dua layar menyebut keadaan yang sama dengan dua kalimat berbeda begitu
salah satunya disunting — dan pada layar yang dipakai menentukan pekerjaan mana yang dikerjakan
lebih dulu, dua sebutan untuk satu keadaan adalah cacat, bukan kosmetik. `internal` dipilih,
bukan `public`: pemakaiannya di dalam assembly aplikasi, dan tidak perlu menjadi permukaan bagi
siapa pun di luarnya.

### 3.4 Selisih terhadap standar endpoint transaksi, dicatat apa adanya

`transaction-endpoint-standard.md` bagian 3 menyatakan dua hal yang bertabrakan dengan kontrak:

| Aturan standar | Kontrak `RAD-API-001` |
| --- | --- |
| `PUT /{id}/<aksi>` — **dilarang** | Menuliskan `PUT /{id}/urgency` |
| Perubahan satu atribut tanpa perpindahan status → `PATCH /{id}/<field>` | Sama seperti di atas |

**Yang dikerjakan mengikuti kontrak.** Dua alasan:

1. Kontrak modul adalah target yang terkunci bagi task ini, dan aturan pemanggilan skill
   menyatakan kontrak approved tidak diubah sepihak.
2. **Seluruh dua belas endpoint aksi pada `RadOrderController` yang sudah berjalan memakai
   `PUT`** — `accept`, `schedule`, `start`, `complete`, `hold`, `resume`, `reject`, `cancel`.
   Menjadikan penanda cito satu-satunya `PATCH` di controller itu akan membuat permukaannya tidak
   dapat ditebak oleh pembuat layar.

Keputusan menyatukannya kembali dengan standar — untuk `urgency` saja, atau untuk seluruh
controller sekaligus — ada pada pemilik modul. Selisihnya dicatat pada `RAD-API-001` revision 10
supaya tidak hilang.

### 3.5 Keputusan pelaksana yang berada di luar teks kontrak

Tiga hal yang tidak ditetapkan kontrak dan diputuskan di sini, seluruhnya dicatat pada
`RAD-API-001` revision 10:

| Keputusan | Alasannya |
| --- | --- |
| `date` kosong berarti **hari ini** | Daftar kerja tanpa batas hari akan memuat seluruh riwayat alat tersebut, dan `GET /worklist` tidak berhalaman |
| Harinya dibaca sebagai tanggal **WIB** | Lihat bagian 2.2 — shift pagi akan melihat pekerjaan kemarin bila dihitung UTC |
| Penanda cito pesanan terminal tidak dapat diubah | Lihat bagian 2.3 |

### 3.6 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Dua endpoint baru**, keduanya sudah tercantum kontrak sebagai rencana. Dengan ini **seluruh endpoint backend roadmap Radiologi ada**. `RAD-API-001` rev 10 dan `RAD-PERM-001` rev 8, **menunggu konfirmasi** |
| Database | **`NOT APPLICABLE`.** Tidak ada entity, configuration, migration, maupun perubahan snapshot. **Tidak ada tabel daftar kerja, dan itu inti slice ini.** Migration `AddRadOrderUrgency` dari `BE-RAD-12` tetap **belum dijalankan** |
| Keamanan/Auth | **Tidak ada string hak akses baru** — `GET /worklist` memakai `RadOrder : Read`, `PUT /{id}/urgency` memakai `RadOrder : Update`, keduanya sudah terdaftar. Setiap perubahan penanda cito masuk audit database beserta pelakunya |

---

## 4. Dokumentasi endpoint

#### Health Services / Radiology Management / Rad Order

Base URL: `api/v1/health-services/radiology-management/rad-orders`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/worklist` | Daftar kerja petugas pada satu alat; pesanan cito di urutan atas | `RadOrder : Read` |
| `PUT` | `/{id}/urgency` | Mengubah penanda cito setelah pesanan dibuat | `RadOrder : Update` |

**Parameter `GET /worklist`:**

| Parameter | Wajib | Keterangan |
| --- | :---: | --- |
| `modalityId` | **Ya** | Alat pencitraan. Tanpanya dijawab `400` |
| `date` | Tidak | Hari kerja, dibaca sebagai tanggal kalender **WIB**. Kosong berarti hari ini |
| `status` | Tidak | Satu keadaan pesanan. Kosong berarti seluruhnya, termasuk yang sudah selesai |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | Berhasil, **0 error**, 188 warning, 2 menit 8 detik | `PASS` | Keluaran perintah |
| Warning baru dari berkas radiologi | **Tidak ada satu pun**; project uji tetap 10 warning | `PASS` | Penyaringan warning build |
| `dotnet build` project uji in-memory | Berhasil, **0 error**, 10 warning | `PASS` | Keluaran perintah |
| `dotnet build` project uji Sqlite | Berhasil, **0 error** | `PASS` | Keluaran perintah |
| `dotnet test --no-build --filter RadWorklistTests` | **24 lulus, 0 gagal**, 12 detik | `PASS` | Keluaran perintah |
| `dotnet test --no-build --filter RadiologyManagement` | **283 lulus, 0 gagal**, 14 detik | `PASS` | 259 sebelumnya + 24 baru |
| `dotnet test --no-build` seluruh project in-memory | **1.488 lulus, 0 gagal**, 34 detik | `PASS` | 1.464 sebelumnya + 24 |
| `dotnet test --no-build` project Sqlite | **481 lulus, 0 gagal**, 6 menit 17 detik | `PASS` | Dijalankan ulang karena `RadOrderService` dan `RadStudyService` disentuh |

### 5.1 Bukti per baris matriks uji penerimaan bagian 7b

| Skenario | Uji | Hasil |
| --- | --- | --- |
| AC-36 — membuka daftar kerja CT-Scan; MRI dan USG tidak muncul | `AC36_DaftarKerjaHanyaMemuatPemeriksaanPadaAlatYangDipilih` | `PASS` |
| AC-37 — query hanya menyentuh `RadOrder` dan `RadStudy`; tidak ada tabel daftar kerja | `AC37_TidakAdaTabelDaftarKerjaPadaModelManaPun`, `AC37_DaftarKerjaHanyaMenyentuhRadOrderDanRadStudy` | `PASS` |
| AC-39 — satu cito dan empat yang lebih tua; cito di urutan pertama | `AC39_SatuPesananCitoBeradaDiUrutanPertamaMendahuluiYangLebihTua` | `PASS` |
| AC-39 — dua cito, keduanya di atas, diurutkan menurut waktu | `AC39_DuaPesananCitoKeduanyaDiAtasDanDiurutkanMenurutWaktu` | `PASS` |
| AC-41 — penanda cito terbaca pada baris study | `AC41_PenandaCitoIkutTerbacaPadaBarisStudy` | `PASS` |
| AC-42 — tersimpan siapa yang menandai dan kapan | `PenandaCitoDapatDipasangSetelahPesananDibuat`; diperkuat `BE-RAD-12` | `PASS` |
| `GET /worklist` tanpa `modalityId` ditolak `400` | `DaftarKerjaTanpaAlatDitolak`, `DaftarKerjaDenganAlatKosongJugaDitolak` | `PASS` |
| AC-40 — penanda terlihat tanpa membuka rincian | Field `IsUrgent` ikut pada setiap baris daftar kerja; **penyajiannya milik `FE-RAD-07`** | `PASS` untuk sisi backend |
| Pemanggil lama membuat pesanan tanpa `IsUrgent` | Dibuktikan `BE-RAD-12` | `PASS` |

### 5.2 Bukti tambahan di luar matriks

| Yang dibuktikan | Uji |
| --- | --- |
| **`FR-RAD-061`** — pesanan dibatalkan langsung hilang tanpa penyelarasan apa pun | `FR061_PesananYangDibatalkanLangsungHilangTanpaPenyelarasan` |
| Penanda study **ikut berubah** ketika penanda pesanannya dicabut — bukti penurunan lebih baik daripada penyalinan | `AC41_PenandaStudyIkutBerubahKetikaPenandaPesanannyaDicabut` |
| Pesanan tanpa study tetap muncul | `PesananTanpaStudyTetapMuncul` |
| Hari kerja WIB — pekerjaan shift pagi masuk hari yang benar | `PekerjaanShiftPagiMasukKeHariKerjaYangBenar` |
| Pekerjaan hari lain tidak ikut terbawa | `PekerjaanHariLainTidakIkutTerbawa` |
| Jadwal mendahului waktu pemesanan dalam menentukan hari kerja | `JadwalMendahuluiWaktuPemesananDalamMenentukanHariKerja` |
| Setiap perubahan penanda masuk riwayat beserta pelakunya, **termasuk pencabutannya** | `SetiapPerubahanPenandaCitoMasukRiwayatBesertaPelakunya` |
| Menetapkan nilai yang sama tidak meninggalkan jejak baru | `MenetapkanPenandaKeNilaiYangSamaTidakMeninggalkanJejakBaru` |
| Penanda pesanan terminal tidak dapat diubah | `PenandaCitoPesananYangSudahSelesaiTidakDapatDiubah`, 3 kasus |
| Bentuk route dan hak akses kedua endpoint sesuai kontrak | `EndpointDaftarKerjaDanPenandaCitoSesuaiKontrak` |

Uji manual: `NOT FEASIBLE` — menuntut aplikasi berjalan beserta database yang kolom citonya belum
ada.

### 5.3 Batas verifikasi yang perlu diketahui

| Yang belum terbukti | Sebabnya |
| --- | --- |
| **Perilaku terhadap database sungguhan** | Migration `AddRadOrderUrgency` belum dijalankan, sehingga ketiga kolom cito belum ada di database mana pun. Seluruh bukti di atas berasal dari penyedia in-memory |
| Index `ModalityId` + `IsUrgent` + `OrderStatus` benar-benar dipakai query | Penyedia in-memory tidak punya rencana eksekusi. Yang terbukti adalah query mengurutkan pada kolom yang sama dengan index-nya |
| Terjemahan `(ScheduledAt ?? RequestedAt ?? CreateDateTime)` ke SQL PostgreSQL | Penyedia in-memory menjalankannya di memori. Pada Postgres ini menjadi `COALESCE` dan **tidak akan memakai index tanggal mana pun** — tidak masalah selama penyaring alat sudah mempersempitnya, tetapi belum diukur |
| Pipeline HTTP sesungguhnya | Uji memanggil service dan memeriksa atribut controller, bukan lewat `WebApplicationFactory` |

**Tidak dijalankan:**

| Yang tidak dijalankan | Alasan |
| --- | --- |
| `dotnet ef migrations add` | **Tidak ada perubahan model.** Daftar kerja memang tidak menambah satu kolom pun |
| `dotnet ef database update` | **Wewenang terpisah dan belum diberikan** |
| Uji integrasi Postgres radiologi | `QUILVIAN_BILLING_TEST_DB` sengaja tidak diisi |
| Analyzer build penuh | Dimatikan atas permintaan pemilik modul. Warning compiler tetap dihitung dan disaring |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-36 — daftar kerja hanya alat yang dipilih | **Terpenuhi** | Tabel 5.1 |
| AC-37 — tidak ada tabel daftar kerja | **Terpenuhi** | Dua uji arsitektur |
| AC-39 — pesanan cito di urutan atas | **Terpenuhi** | Dua uji urutan |
| AC-41 — penanda cito terbaca pada baris study | **Terpenuhi** | Dua uji, termasuk yang membuktikan penandanya ikut berubah |
| **DoD — daftar kerja hanya menyentuh `RadOrder` dan `RadStudy`** | **Terpenuhi** | `AC37_DaftarKerjaHanyaMenyentuhRadOrderDanRadStudy` membaca source methodnya sendiri dan memastikan satu-satunya DbSet yang disentuh adalah `RadOrders`, dengan study dicapai lewat navigasi |

**Butir yang belum terpenuhi, disebut apa adanya:**

1. **AC-38 belum tersentuh** — "petugas dapat berpindah antar daftar alat tanpa berganti halaman"
   adalah kriteria layar, milik `FE-RAD-07`. Backend menyediakan satu endpoint berparameter alat,
   yang memang bentuk yang dibutuhkan layar seperti itu.
2. **AC-40 hanya terpenuhi di backend** — field `IsUrgent` ikut pada setiap baris; penyajiannya
   agar "terlihat tanpa membuka rincian" milik `FE-RAD-05` dan `FE-RAD-07`.
3. **Belum ada bukti terhadap database sungguhan**, karena migration `BE-RAD-12` belum dijalankan.
4. **Penghalang `RadReport : ActAsRadiologist` masih terbuka** — tidak berkaitan dengan task ini,
   tetapi masih menunggu keputusan Anda.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru dari berkas radiologi |
| Masalah yang diketahui | Selisih verb terhadap `transaction-endpoint-standard.md` — bagian 3.4. Cacat `MapDetail` yang dilaporkan `BE-RAD-12` **belum diperbaiki** dan masih berlaku |
| Risiko tersisa | **Pertama**, migration `AddRadOrderUrgency` belum dijalankan, sehingga `GET /worklist` akan gagal pada database yang belum punya kolom `IsUrgent`. **Kedua**, `GET /worklist` tidak berhalaman; bawaan "hari ini" yang membatasinya, dan pemanggil yang mengirim `date` lama pada alat sibuk akan menerima daftar panjang. **Ketiga**, penanda cito dapat diubah siapa pun yang memegang `RadOrder : Update` — tidak ada pembatasan peran, dan kontrak memang tidak memintanya |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat bagian 7.2 |
| Langkah berikutnya | Lihat bagian 7.1 |

### 7.1 Keadaan roadmap backend Radiologi setelah task ini

**Seluruh 15 task backend pada `RAD-RM-BE-001` selesai.**

| Gelombang | Task | Keadaan |
| --- | --- | --- |
| `MVP-0` | `BE-RAD-01` s/d `BE-RAD-06` | Selesai |
| `MVP-1` | `BE-RAD-15` | Selesai sebagian — tertahan `DEC-RAD-005`, pengesahan aturan oleh penanggung jawab klinis |
| `MVP-2` | `BE-RAD-07` s/d `BE-RAD-10` | Selesai |
| `MVP-4` | `BE-RAD-11` s/d `BE-RAD-13` | Selesai |
| Lintas | `BE-RAD-14` | Selesai |

> **Keempatnya sudah diputuskan dan dikerjakan pada hari yang sama, 2026-09-11.** Lihat
> `approval-requests/2026-09-11-keputusan-empat-penghalang.md`. Ringkasnya: penghalang
> `ActAsRadiologist` **ditutup**; kedua migration **diserahkan sebagai skrip SQL** karena
> targetnya database non-lokal; lima amandemen kontrak **disetujui**; kalimat `RAD-STATE-001`
> bagian 3 **diperbaiki**. Tabel di bawah dibiarkan apa adanya sebagai catatan keadaan saat
> `BE-RAD-13` ditutup.

**Empat hal menunggu keputusan Anda, dan tidak satu pun dapat diselesaikan backend sendiri:**

| Yang menunggu | Rinciannya |
| --- | --- |
| **Penanda `RadReport : ActAsRadiologist` belum dapat diberikan** | `BE-RAD-08.md` bagian 7.1. Selama ini belum selesai, pengesahan dan perilisan hasil bacaan menolak semua orang kecuali SuperAdmin — aturan `RAD-DEC-003` ada di kode tetapi belum berjalan |
| **Dua migration belum dijalankan** | `AddRadReport` dan `AddRadOrderUrgency`. Selama itu tabel hasil bacaan belum ada dan daftar kerja akan gagal |
| **Lima amandemen kontrak menunggu konfirmasi** | `RAD-API-001` revision 6 s/d 10, `RAD-PERM-001` revision 6 s/d 8 |
| **Satu kalimat kontrak perlu diperbaiki** | `RAD-STATE-001` bagian 3 tentang kapan versi lama menjadi `Superseded` — `BE-RAD-10.md` bagian 2.3 |

Ditambah dua usulan yang sudah tiga kali dilaporkan: **squash migration** (folder `Migrations`
kini 404 MB), dan `RadReport.Version` yang didokumentasikan sebagai token konkurensi tetapi belum
dideklarasikan `IsConcurrencyToken()`.

### 7.2 Status Git pada akhir pekerjaan

Berkas hasil task ini:

```text
 M Areas/HealthServices/RadiologyManagement/Controllers/RadOrderController.cs
 M Areas/HealthServices/RadiologyManagement/DTOs/RadiologyDtos.cs
 M Areas/HealthServices/RadiologyManagement/Services/RadOperationResult.cs
 M Areas/HealthServices/RadiologyManagement/Services/RadOrderService.cs
 M Areas/HealthServices/RadiologyManagement/Services/RadStudyService.cs
 M docs/module-blueprints/radiologi/contracts/api-contract.md
 M docs/module-blueprints/radiologi/contracts/permission-audit-matrix.md
 M docs/module-blueprints/radiologi/roadmap/backend-roadmap.md
 M docs/module-blueprints/radiologi/roadmap/requirement-traceability.md
?? Tests/QuilvianSystemBackend.UnitTests.InMemory/RadiologyManagement/RadWorklistTests.cs
?? docs/module-blueprints/radiologi/task/report/backend/BE-RAD-13.md
```

Berkas lain yang tampak pada `git status` — Laboratorium, laporan `BE-RAD-04` sampai `BE-RAD-12`,
kedua migration, serta seluruh berkas hasil bacaan — sudah ada sebelum task ini dimulai dan
**bukan** hasil pekerjaan ini.

Tidak ada `git add`, commit, maupun push yang dilakukan. **Tidak ada perintah database yang
dijalankan.**
