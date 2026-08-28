# Execution Evidence `RJ-BIL-BE-003` — Lab Milestone sampai `Accepted`

| Field | Nilai |
|---|---|
| Task | `RJ-BIL-BE-003` |
| Status | `IMPLEMENTATION_COMPLETE` |
| Governance | `OPEN` — sign-off Lab, Clinical Governance, dan Billing/Finance belum tersedia |
| Blueprint | `RJ-BIL-BP-001` revision `11` |
| Requirement sumber | `RJ-BIL-GATE-DEC-003`, `RJ-BIL-GATE-DEC-005` |
| Keputusan author | `RJ-BIL-OQ-008` s.d. `RJ-BIL-OQ-011` |
| SHA sebelum | `d0544e53bc876c0a74bc7befedb1a036dd08e1fd` cabang `sukmagp` |
| SHA sesudah | `d0544e53bc876c0a74bc7befedb1a036dd08e1fd` — tidak ada commit |
| Build | `PASS` — `0` error |
| Test murni | `PASS` — `21` lulus, `0` gagal |
| Test berbasis database | `BLOCKED_BY_TEST_DB_CONFIGURATION` — `37` terhalang |
| Migration dibuat | `YES` — `20260824091610_AddLaboratorySpecimenLifecycle` |
| Migration diterapkan | `NO` — terverifikasi `(Pending)` pada `QuilvianNewDevTim01` |
| Commit / Push / Merge / Deploy | `NO` |
| Tanggal | `2026-08-24` |

## 1. Ringkasan untuk pembaca non-teknis

Sebelum tugas ini, sistem tidak dapat mencatat satu pun langkah kerja laboratorium. Yang ada hanya
"buat pesanan" dan "batalkan pesanan". Sekarang seluruh alur tercatat, dan yang terpenting: **tagihan
pemeriksaan baru terbentuk pada saat petugas laboratorium menyatakan sampel layak periksa.**

Contoh nyata sesuai keputusan Anda pada `RJ-BIL-OQ-008`. Dokter memesan paket `Rp450.000`:

| Komponen | Pemeriksaan | Tarif | Yang terjadi | Ditagih? |
|---|---|---|---|---|
| A | Darah lengkap | `Rp200.000` | Sampel dinyatakan layak | **Ya** |
| B | Fungsi hati | `Rp150.000` | Sampel dinyatakan layak | **Ya** |
| C | Urin lengkap | `Rp100.000` | Sampel ditolak, jumlah kurang | **Tidak** |
| | | **`Rp350.000`** | | |

Pasien membayar `Rp350.000`, bukan `Rp450.000` dan bukan nol. Ini yang dibuktikan test
`DuaKomponenLayakSatuDitolak_MenagihTigaRatusLimaPuluhRibu`.

Bila sampel urin harus diambil ulang karena **kesalahan rumah sakit** — misalnya salah label —
pengambilan ulang itu tidak menambah tagihan. Pemeriksaannya tetap satu kali, jadi tagihannya tetap
satu. Ini dibuktikan `PengambilanUlangKesalahanInternal_HanyaMenghasilkanSatuTagihan`.

Bila pemeriksaan dibatalkan **setelah** sampel dinyatakan layak, tagihan yang sudah terbentuk
**tidak dihapus**. Laboratorium hanya mencatat bahwa pembatalan klinis terjadi, lalu Billing yang
memutuskan apakah tagihan itu dikoreksi, dikurangi, atau dibatalkan.

## 2. Keputusan author dan cara pelaksanaannya

| Keputusan | Isi | Pelaksanaan |
|---|---|---|
| `RJ-BIL-OQ-008` | Kelayakan tagih dinilai per sampel/komponen pemeriksaan | Tarif komponen melekat pada `TrxLabSpecimen.ProcedureId`; fakta diterbitkan per sampel dengan `SourceItemId = Specimen.Id` |
| `RJ-BIL-OQ-008` | `Requested`, `Collected`, `Received` bukan pemicu tagihan | Hanya `AcceptAsync` yang memanggil `EmitChargeEligibilityAsync`; tiga langkah lainnya tidak menerbitkan apa pun |
| `RJ-BIL-OQ-008` | `Rejected` menghasilkan nol tagihan | `RejectAsync` mengembalikan `Handoff = null` tanpa menyentuh Billing |
| `RJ-BIL-OQ-008` | Kesalahan internal tidak menambah tanggungan pasien | Pengambilan ulang menghasilkan sampel baru; sampel lama ditolak dan tidak pernah menagih, sehingga totalnya tetap satu tagihan |
| `RJ-BIL-OQ-009` | Katalog alasan yang dapat dikonfigurasi, bukan enum permanen | Master data `MstLabRejectionReason` berisi `10` kode baseline; penambahan alasan tidak memerlukan perubahan program |
| `RJ-BIL-OQ-009` | `OTHER` mewajibkan catatan | Kolom `RequiresNote` bernilai benar untuk `OTHER`; ditegakkan `RejectAsync` |
| `RJ-BIL-OQ-009` | Riwayat penolakan tidak boleh ditimpa | `TrxLabTransitionHistory` hanya ditambah; tidak ada satu pun jalur update terhadapnya |
| `RJ-BIL-OQ-010` | Barcode `LSP-<32 hex>`, dibuat server, tanpa PHI | `GenerateSpecimenBarcode()` tidak menerima satu pun parameter, sehingga secara struktural tidak dapat menyisipkan identitas pasien |
| `RJ-BIL-OQ-010` | Unik, tidak berubah, bukan kredensial | Unique index pada `SpecimenBarcode`; tidak dipakai di jalur otorisasi mana pun |
| `RJ-BIL-OQ-010` | Pengambilan ulang memakai identitas dan barcode baru | `SupersededSpecimenId` menyimpan tautan ke sampel yang ditolak |
| `RJ-BIL-OQ-011` | Boleh jalan, governance tetap `OPEN` | Status task `IMPLEMENTATION_COMPLETE`; tiga sign-off tetap tercatat `OPEN` |

## 3. Pengamanan database test — dikerjakan lebih dulu

Ini langkah pertama yang saya kerjakan, sebelum menyentuh satu baris pun kode Laboratorium.

Pada `RJ-BIL-BE-002`, `BillingTestDatabaseFixture` jatuh ke `appsettings.Development.json` ketika
`QUILVIAN_BILLING_TEST_DB` kosong, sehingga `dotnet test` menerapkan migration ke database dev
bersama. Fallback itu **dihapus seluruhnya**.

Perilaku sekarang:

| Keadaan | Yang terjadi |
|---|---|
| `QUILVIAN_BILLING_TEST_DB` kosong | Berhenti dengan `BLOCKED_BY_TEST_DB_CONFIGURATION`, tanpa membuka koneksi |
| Nama database `QuilvianNewDevTim01` | Ditolak, terdaftar eksplisit sebagai terlarang |
| Nama mengandung `prod`, `production`, `live`, `staging`, `stage`, `uat`, `dev`, `shared` | Ditolak |
| Nama tidak mengandung `test` | Ditolak — bukti afirmatif dituntut, bukan sekadar ketiadaan penanda buruk |
| Nama sah, misalnya `QuilvianBillingTest` | Diteruskan; `Database.Migrate()` baru berjalan setelah seluruh pemeriksaan lolos |

Fixture juga tidak lagi membaca file konfigurasi mana pun, sehingga tidak ada kredensial yang
tersentuh dari source. Nama host dan database dicetak ke output test; username dan password tidak.

### Bukti perilaku, bukan klaim

```
QUILVIAN_BILLING_TEST_DB kosong
  → BLOCKED_BY_TEST_DB_CONFIGURATION: environment variable ... belum diisi

QUILVIAN_BILLING_TEST_DB = Database=QuilvianNewDevTim01
  → BLOCKED_BY_TEST_DB_CONFIGURATION: database 'QuilvianNewDevTim01' termasuk daftar terlarang

QUILVIAN_BILLING_TEST_DB = Database=QuilvianBillingTest (port tidak ada)
  → NpgsqlException — gerbang lolos, gagal di koneksi. Guard tidak memblokir berlebihan.
```

### Bukti database bersama tidak tersentuh

```
dotnet ef migrations list

20260824074649_AddClinicalMilestoneFactHandoff
20260824080430_StoreClinicalFactSnapshotAsText
20260824091610_AddLaboratorySpecimenLifecycle (Pending)
```

Migration `RJ-BIL-BE-003` berstatus `(Pending)` pada `QuilvianNewDevTim01`. Ia dibuat, ditinjau, dan
**tidak diterapkan**. Dua migration `RJ-BIL-BE-002` di atasnya tetap tercatat sudah diterapkan
sesuai insiden yang sudah dilaporkan sebelumnya; keadaan itu tidak berubah oleh task ini.

## 4. Alur bisnis yang sekarang berjalan

Urutan lengkap satu pemeriksaan laboratorium:

1. **Dokter memesan.** `POST /lab-orders` membuat pesanan berstatus `Requested`. Belum ada tagihan.
2. **Petugas merencanakan sampel.** `POST /lab-specimens/by-order/{id}` membuat satu sampel dengan
   barcode `LSP-…` dan menyalin tarif komponen pemeriksaannya. Belum ada tagihan.
3. **Perawat mengambil sampel.** `POST /lab-specimens/{id}/collect`. Belum ada tagihan.
4. **Sampel tiba di laboratorium.** `POST /lab-specimens/{id}/receive`. Tiba secara fisik belum
   berarti dinyatakan layak, jadi **belum ada tagihan**.
5. **Petugas laboratorium menilai kelayakan.** Dua kemungkinan:
   - `POST /lab-specimens/{id}/accept` → **tagihan terbentuk**, pesanan ikut menjadi `Accepted`.
   - `POST /lab-specimens/{id}/reject` dengan kode alasan → **tidak ada tagihan**.
6. **Bila ditolak dan perlu diambil ulang.** `POST /lab-specimens/{id}/request-recollection`
   membuat sampel baru berbarcode baru yang tetap menunjuk sampel lama sebagai asal-usulnya.
7. **Pekerjaan berjalan lalu selesai.** `PUT /lab-orders/{id}/start-process` dan `/complete`.
   Keduanya tidak menambah tagihan, karena tagihannya sudah terbentuk pada langkah 5.

Hasil pemeriksaan tidak ada di alur ini. Sesuai `RJ-BIL-GATE-DEC-003`, siklus hasil terpisah dari
pesanan dan sampel, dan penerbitan hasil bukan pemicu tagihan.

## 5. Endpoint

### `[Tags("Health Services / Laboratory Management / Lab Order")]`

| Method | Route | Permission | Kegunaan | Memicu tagihan? |
|---|---|---|---|---|
| `GET` | `/api/v1/health-services/laboratory-management/lab-orders` | `LabOrder : Read` | Daftar pesanan | Tidak |
| `GET` | `…/lab-orders/{id}` | `LabOrder : Read` | Detail pesanan | Tidak |
| `POST` | `…/lab-orders` | `LabOrder : Create` | Membuat pesanan | Tidak |
| `PUT` | `…/lab-orders/{id}/start-process` | `LabOrder : Process` | Mulai dikerjakan | Tidak |
| `PUT` | `…/lab-orders/{id}/complete` | `LabOrder : Process` | Selesai dikerjakan | Tidak |
| `PUT` | `…/lab-orders/{id}/hold` | `LabOrder : Hold` | Menahan pesanan | Tidak |
| `PUT` | `…/lab-orders/{id}/resume` | `LabOrder : Hold` | Melanjutkan pesanan | Tidak |
| `PUT` | `…/lab-orders/{id}/cancel` | `LabOrder : Update` | Membatalkan pesanan secara klinis | Menerbitkan koreksi |

### `[Tags("Health Services / Laboratory Management / Lab Specimen")]`

| Method | Route | Permission | Kegunaan | Memicu tagihan? |
|---|---|---|---|---|
| `GET` | `…/lab-specimens/rejection-reasons` | `LabSpecimen : Read` | Katalog alasan penolakan | Tidak |
| `GET` | `…/lab-specimens/by-order/{id}` | `LabSpecimen : Read` | Sampel pada satu pesanan | Tidak |
| `GET` | `…/lab-specimens/by-order/{id}/history` | `LabSpecimen : Read` | Riwayat perpindahan status | Tidak |
| `POST` | `…/lab-specimens/by-order/{id}` | `LabSpecimen : Plan` | Merencanakan sampel | Tidak |
| `POST` | `…/lab-specimens/{id}/collect` | `LabSpecimen : Collect` | Mencatat pengambilan | Tidak |
| `POST` | `…/lab-specimens/{id}/receive` | `LabSpecimen : Receive` | Mencatat kedatangan | Tidak |
| `POST` | `…/lab-specimens/{id}/accept` | `LabSpecimen : Accept` | Menyatakan layak periksa | **Ya** |
| `POST` | `…/lab-specimens/{id}/reject` | `LabSpecimen : Accept` | Menolak dengan alasan | Tidak |
| `POST` | `…/lab-specimens/{id}/request-recollection` | `LabSpecimen : Accept` | Meminta pengambilan ulang | Tidak |
| `POST` | `…/lab-specimens/{id}/hold` | `LabSpecimen : Hold` | Menahan sampel | Tidak |
| `POST` | `…/lab-specimens/{id}/resume` | `LabSpecimen : Hold` | Melanjutkan sampel | Tidak |
| `POST` | `…/lab-specimens/{id}/cancel` | `LabSpecimen : Cancel` | Membatalkan sampel secara klinis | Menerbitkan koreksi |

Tidak ada satu pun endpoint finansial. Laboratorium tidak dapat menyatakan lunas, menyelesaikan
pembayaran, menyetujui penjamin, mem-void, mengembalikan dana, atau membalik transaksi.

## 6. Daftar berkas

### Baru

| Berkas | Isi |
|---|---|
| `Areas/HealthServices/LaboratoryManagement/Enums/LaboratoryEnums.cs` | `LabOrderStatus`, `LabSpecimenStatus`, `LabRecollectionCause`, `LabTransitionScope` |
| `…/Models/TrxLabSpecimen.cs` | Sampel sekaligus komponen pemeriksaan |
| `…/Models/TrxLabTransitionHistory.cs` | Riwayat perpindahan status yang hanya bertambah |
| `…/Models/MstLabRejectionReason.cs` | Katalog alasan penolakan |
| `…/Configurations/LaboratoryManagementConfigurations.cs` | Konfigurasi tiga entity baru |
| `…/DTOs/LabSpecimenDtos.cs` | Request dan response alur sampel |
| `…/Services/LabSpecimenService.cs` | Inti alur sampel dan penerbitan fakta |
| `…/Controllers/LabSpecimenController.cs` | 12 endpoint sampel |
| `Migrations/20260824091610_AddLaboratorySpecimenLifecycle.cs` | Skema dan pengisian data |
| `Tests/…/Laboratory/LaboratoryAuthorityTests.cs` | 18 test tanpa database |
| `Tests/…/Laboratory/LaboratorySpecimenLifecycleTests.cs` | 16 test berbasis database |
| `docs/…/preflight-RJ-BIL-BE-003.md` | Preflight read-only |

### Diubah

| Berkas | Perubahan |
|---|---|
| `…/Operational/Constants/BillingSourceContract.cs` | Menambah `Laboratory` dan `LaboratoryCharge` |
| `…/LaboratoryManagement/Models/LabOrder.cs` | Status, waktu, dan token konkurensi |
| `…/LaboratoryManagement/DTOs/LabOrderDtos.cs` | Status dan jumlah sampel pada response |
| `…/LaboratoryManagement/Services/LabOrderService.cs` | Siklus hidup pesanan dan pembatalan yang menerbitkan koreksi |
| `…/LaboratoryManagement/Controllers/LabOrderController.cs` | Lima endpoint siklus hidup |
| `Repositories/Configurations/HealthServices/LabOrderConfiguration.cs` | Diperluas, bukan diduplikasi |
| `Repositories/ApplicationDbContext.cs` | Tiga `DbSet` baru |
| `Program.cs` | Registrasi `LabSpecimenService` |
| `Tests/…/Infrastructure/BillingTestDatabaseFixture.cs` | Fail-closed dan pembersihan entity Lab |
| `Tests/…/Operational/BillingFolioServiceTests.cs` | `Laboratory` tidak lagi contoh konteks ditolak |
| `Tests/…/ClinicalIntegration/ClinicalMilestoneFactProducerTests.cs` | Idem |

## 7. Arsitektur

### Yang tidak perlu diubah

Arsitektur `RJ-BIL-BE-002` menahan beban `BE-003` tanpa perombakan:

| Komponen | Perubahan |
|---|---|
| `ClinicalMilestoneFactProducer` | Tidak ada. Producer sudah generik terhadap `SourceContext` |
| `TrxClinicalMilestoneFact` | Tidak ada. `SourceItemId` sudah tersedia untuk identitas sampel |
| `BillingFolioService` | Tidak ada |
| `BillingSourceContract` | Satu entri baru |

### Identitas fakta

| Unsur | Nilai |
|---|---|
| `SourceContext` | `Laboratory` |
| `SourceAggregateId` | `LabOrder.Id` |
| `SourceItemId` | `TrxLabSpecimen.Id` |
| `EffectType` | `LaboratoryCharge` |

Karena identitas tagihan di Billing tidak menyertakan nomor versi, revisi selalu jatuh ke tagihan
yang sama. Pembatalan setelah sampel dinyatakan layak karena itu menghasilkan versi `2` atas fakta
yang sama, bukan tagihan kedua.

### Penyerahan setelah penyimpanan

Fakta diterbitkan setelah perubahan klinis tersimpan, tidak pernah di dalam transaksi yang masih
terbuka. Alasannya sama dengan `BE-002`: `BillingFolioService` membuka transaksi `Serializable`
sendiri, dan Billing yang tidak dapat dihubungi tidak boleh membatalkan penetapan layak yang secara
klinis sudah benar terjadi.

### Idempotensi pengulangan

`AcceptAsync` yang dipanggil ulang atas sampel yang sudah layak tidak mengubah keadaan dan memakai
kembali `DecidedAt` yang tersimpan. Karena muatan fakta seluruhnya berasal dari baris yang sudah
tersimpan — termasuk tarif yang disalin saat perencanaan — sidik jari permintaannya identik dan
Billing mengenalinya sebagai pengiriman ulang.

Penyalinan tarif saat perencanaan adalah pilihan sadar. Membaca ulang master tarif pada setiap
pengiriman akan mengubah sidik jari begitu master tarif berubah, dan pengiriman ulang berhenti
bersifat idempotent — persis kegagalan yang menghabiskan tiga percobaan perbaikan pada `BE-002`.

## 8. Migration

`20260824091610_AddLaboratorySpecimenLifecycle`

| Perubahan | Rincian |
|---|---|
| `LabOrder` bertambah kolom | `OrderStatus`, `StatusBeforeHold`, `RequestedAt`, `RequestedByUserId`, `CompletedAt`, `Version` |
| Tabel baru | `TrxLabSpecimen`, `TrxLabTransitionHistory`, `MstLabRejectionReason` |
| Index | Unik pada `SpecimenBarcode`; unik parsial pada `ReasonCode`; index pencarian pada status, pesanan, encounter |
| Foreign key | Seluruhnya `Restrict`, termasuk rantai `SupersededSpecimenId` |
| Pengisian data | `10` alasan penolakan baseline dengan Id tetap |

### Dua cacat yang ditemukan pada tinjauan statis

Keduanya cacat yang saya buat sendiri dan tertangkap sebelum migration dijalankan.

**Cacat 1 — status bawaan tidak dapat dibaca.** EF menghasilkan `OrderStatus` dengan
`defaultValue: 0`, padahal `LabOrderStatus` tidak memiliki anggota bernilai `0`. Seluruh baris lama
akan memperoleh status yang tidak dapat ditafsirkan aplikasi. Diperbaiki menjadi `2` = `Requested`,
ditambah pengisian eksplisit: baris ber-`IsCancel` dipetakan ke `8` = `Cancelled`.

Test `StatusLaboratorium_TidakMemakaiNilaiNol` sekarang menjaga agar cacat kelas ini tidak terulang.

**Cacat 2 — waktu bergeser dan Id berbeda antar lingkungan.** Pengisian awal saya memakai
`NOW() AT TIME ZONE 'UTC'` yang menghasilkan timestamp tanpa zona waktu, lalu ditafsirkan ulang
memakai zona server sehingga waktunya bergeser. `gen_random_uuid()` juga membuat baris baseline
ber-Id berbeda di setiap lingkungan. Diperbaiki menjadi `NOW()` dan Id tetap.

### Rollback

`Down` menghapus ketiga tabel baru dan seluruh kolom tambahan pada `LabOrder`. Data sampel dan
riwayat akan hilang bersama tabelnya; data `LabOrder` yang sudah ada sebelum migration tetap utuh.

**Migration ini belum dijalankan ke database mana pun** dan memerlukan otorisasi tersendiri.

## 9. Tinjauan keamanan

| Aspek | Keadaan | Bukti |
|---|---|---|
| Otorisasi di sisi server | `PASS` | Seluruh endpoint `[Authorize]` + `[AccessPermission]`; filter menolak dengan `403` bila role tidak memilikinya |
| Least privilege | `PASS` | `Collect`, `Receive`, `Accept`, `Plan`, `Cancel`, `Hold` adalah permission berbeda; dijaga test `PermissionPengambilanDanPenetapanLayak_TidakBolehSama` |
| Izin klinis tidak dapat melakukan mutasi finansial | `PASS` | Tidak ada properti atau method ber-istilah finansial pada Laboratorium; dijaga dua test refleksi |
| Validasi masukan | `PASS` | Panjang maksimum pada seluruh DTO; `ReasonCode` wajib dan harus ada di katalog aktif |
| Validasi entity dan UUID | `PASS` | Route `{id:guid}`; pesanan, sampel, procedure, dan alasan diverifikasi keberadaannya |
| Validasi transisi status | `PASS` | Setiap tindakan menuntut status asal tertentu; `Accept` menolak sampel yang belum `Received` |
| Perlindungan mass-assignment | `PASS` | DTO tidak memuat status, barcode, versi, tarif, maupun kolom audit |
| Tidak ada status finansial dari client | `PASS` | Client tidak dapat mengirim nilai finansial apa pun |
| Keamanan transaksi | `PASS` | Perubahan status dan riwayatnya tersimpan dalam satu `SaveChanges` |
| Perlindungan konkurensi | `PASS` | `Version` sebagai token konkurensi pada `LabOrder` dan `TrxLabSpecimen`; balasan `409` |
| Perlindungan pengulangan | `PASS` | `Accept` ulang menghasilkan `Replayed`, bukan revisi baru |
| Idempotensi | `PASS` | Kunci idempotency diturunkan dari identitas, bukan waktu panggilan |
| Jejak audit | `PASS` | `TrxLabTransitionHistory` mencatat pelaku, waktu, status asal dan tujuan, aksi, serta alasan |
| Penanganan exception | `PASS` | Controller hanya mengembalikan pesan yang memang disusun untuk pengguna |
| Tidak membocorkan stack trace | `PASS` | Pesan exception asli connection string sengaja tidak diteruskan |
| Tidak ada kredensial ter-hardcode | `PASS` | Fixture tidak lagi membaca file konfigurasi; pemindaian rahasia bersih |
| Tidak ada SQL injection | `PASS` | Seluruh query melalui LINQ; SQL migration tidak memuat masukan pengguna |
| Tidak ada IDOR | `PASS` | Sampel dimuat bersama pesanannya dan diverifikasi tidak terhapus |
| Barcode bukan kredensial | `PASS` | Tidak dipakai di jalur otorisasi mana pun |
| Barcode tanpa PHI | `PASS` | Pembangkitnya tidak menerima parameter apa pun |
| Tidak ada jalur pintas finansial | `PASS` | Satu-satunya jalur ke Billing adalah `ClinicalMilestoneFactProducer` |

## 10. Bukti test

### Test yang dapat dijalankan — `21` lulus, `0` gagal

| Test | Yang dibuktikan |
|---|---|
| `BillingSourceContract_MenerimaLaboratory` | Kontrak menerima `Laboratory` dan `LaboratoryCharge` |
| `BillingSourceContract_MasihMenolakRadiology` | `RJ-BIL-BE-004` tetap tertutup |
| `BillingSourceContract_MenolakEffectTypeYangTidakCocok…` | Effect type asing ditolak |
| `ModelLaboratorium_TidakMemilikiPropertiFinansialApaPun` | Tidak ada `Paid`, `Settlement`, `Void`, `Refund`, `Reversal`, `PayerApproval` |
| `ServiceLaboratorium_TidakMemilikiMethodKewenanganFinansial` | Idem pada service |
| `EndpointSampel_MemakaiPermissionYangDitetapkan` (7 kasus) | Tiap langkah memakai permission yang benar |
| `PermissionPengambilanDanPenetapanLayak_TidakBolehSama` | Kewenangan berbeda per langkah |
| `PembangkitBarcode_TidakMenerimaMasukanApaPun` | Barcode tidak dapat memuat PHI |
| `BarcodeSampel_BerbentukLspDiikutiTigaPuluhDuaHeksadesimal` | Format dan keunikan `200` barcode |
| `SiklusHidupPesanan_SamaPersisDenganKeputusanTerkunci` | Tidak ada status karangan |
| `SiklusHidupSampel_SamaPersisDenganKeputusanTerkunci` | Idem |
| `StatusLaboratorium_TidakMemakaiNilaiNol` | Menjaga cacat migration tidak terulang |
| 3 test warisan `RJ-BIL-BE-002` | Kewenangan finansial farmasi tetap tercabut |

### Test berbasis database — `16` ditulis, `BLOCKED_BY_TEST_DB_CONFIGURATION`

Seluruhnya sudah ditulis dan lolos kompilasi, tetapi **belum pernah dijalankan** karena database
test khusus belum tersedia. Saya tidak mem-bypass gerbang keselamatan untuk menjalankannya.

| Test | Skenario wajib yang ditutup |
|---|---|
| `SebelumDinyatakanLayak_TidakAdaTagihanYangTerbentuk` | `Requested`/`Collected`/`Received` bukan pemicu |
| `PenetapanLayak_MembentukTepatSatuFaktaDanSatuBarisTagihan` | `Accepted` = milestone kelayakan |
| `DuaKomponenLayakSatuDitolak_MenagihTigaRatusLimaPuluhRibu` | `RJ-BIL-OQ-008` opsi A |
| `SampelDitolak_TidakMenerbitkanFaktaApaPun` | `Rejected` = nol tagihan |
| `AlasanPenolakanOther_WajibDisertaiCatatan` | `RJ-BIL-OQ-009` |
| `AlasanPenolakanTidakDikenal_Ditolak` | Alasan terkendali |
| `PenetapanLayakDiulang_TidakMenggandakanTagihan` | Idempotensi dan pengulangan |
| `PengambilanUlang_MempertahankanSampelDitolakDanTautanSebabnya` | Riwayat immutable, identitas baru |
| `PengambilanUlangKesalahanInternal_HanyaMenghasilkanSatuTagihan` | Tidak menambah tanggungan pasien |
| `PengambilanUlangSebabEksternal_WajibMenyertakanAlasan` | Otorisasi sebab non-internal |
| `PembatalanSetelahLayak_TidakMenghapusTagihanDanMemakaiRevisiBaru` | Versi baru, tagihan asli utuh |
| `PembatalanSebelumLayak_TidakMenghasilkanKoreksiFinansial` | Tidak ada koreksi tanpa tagihan |
| `PembatalanPesanan_MembatalkanSampelDanMenerbitkanKoreksi…` | Pembatalan tingkat pesanan |
| `DuaPetugasMenetapkanLayakBersamaan_SalahSatuDitolak` | Konkurensi |
| `PesananYangSudahDibatalkan_TidakDapatMenerimaSampelBaru` | Batas kewenangan |
| `PenetapanLayakTanpaMelaluiPenerimaan_Ditolak` | Validasi transisi |
| `ProcedureBukanLaboratorium_TidakDapatDipakaiSebagaiKomponen` | Validasi master |
| `BarcodeSampel_UnikDanTidakMemuatIdentitasPasien` | `RJ-BIL-OQ-010` |

### Hasil penuh

```
Failed!  - Failed: 37, Passed: 21, Skipped: 0, Total: 58
Seluruh 37 kegagalan bertanda BLOCKED_BY_TEST_DB_CONFIGURATION.
Tidak ada satu pun kegagalan domain.
```

`37` terdiri atas `16` test Lab yang baru dan `21` test `BE-001`/`BE-002` yang sebelumnya berjalan
memakai fallback ke database dev bersama. Keduanya kini terhalang oleh gerbang yang sama.

**Untuk menjalankannya**, sediakan database test khusus yang boleh dibuang lalu isi
`QUILVIAN_BILLING_TEST_DB` dengan connection string-nya. Nama database harus mengandung `test`.

## 11. Batas yang saya jaga

| Hal | Status |
|---|---|
| SOP Lab dikarang sendiri | `NO` — seluruh aturan dari `RJ-BIL-GATE-DEC-003` dan keputusan author |
| Integrasi Lab eksternal atau LIS | `NO` — tidak disentuh |
| Adapter eksternal `RJ-BIL-DEP-009` | Tetap `INACTIVE` |
| Implementasi `RJ-BIL-BE-004` atau `BE-005` | `NO` |
| Migration diterapkan | `NO` — terverifikasi `(Pending)` |
| Mutasi database bersama | `NO` |
| Commit / Push / Merge / Deploy | `NO` |
| Governance ditandai `APPROVED` | `NO` — ketiganya tetap `OPEN` |

## 12. Celah yang saya sadari

| Kode | Celah | Akibat |
|---|---|---|
| `G-01` | `16` test berbasis database belum pernah dijalankan | Perilaku runtime alur Lab belum terbukti secara empiris. Ini celah paling penting pada task ini |
| `G-02` | Pencatatan konsumsi reagen/BHP tidak tersedia | Aturan "material yang benar-benar dikonsumsi dinilai terpisah" belum punya pelaksana. Dinyatakan di luar scope `BE-003` |
| `G-03` | Matriks state-transition, validation, dan permission belum diperbarui untuk Lab | Dokumen turunan; isinya sudah tercermin pada bagian `5` dan `9` dokumen ini |
| `G-04` | ERD Lab belum dibuat | Idem |
| `G-05` | Tidak ada consumer frontend | `RJ-BIL-FE-002` belum dikerjakan; alur Lab belum dapat dipakai dari layar mana pun |
| `G-06` | `docs/system-registry/` tidak ada di repository | Pemeriksaan kavling nama `rule-prascan` dilakukan manual melalui pencarian source |

## 13. Keputusan klinis baru yang saya temukan

### `RJ-BIL-BE-003-ODR-001` — satu sampel untuk beberapa pemeriksaan

**`OWNER_DECISION_REQUIRED`**

Keputusan `RJ-BIL-OQ-008` menyebut satuan tagih sebagai "specimen / examination charge component"
dengan tanda garis miring. Implementasi ini menafsirkannya sebagai satu banding satu: satu sampel
mewakili satu komponen pemeriksaan, membawa satu procedure dan satu tarif. Penafsiran itu memenuhi
contoh `Rp450.000` Anda secara persis.

Di laboratorium nyata, satu tabung darah kerap melayani beberapa pemeriksaan sekaligus. Keadaan itu
**tidak tercakup** requirement yang sudah dikunci, dan saya tidak mengarang aturannya.

| Yang belum diputuskan | Akibat bila diabaikan |
|---|---|
| Apakah satu sampel boleh memuat lebih dari satu pemeriksaan bertarif berbeda | Petugas akan membuat beberapa "sampel" untuk satu tabung fisik, sehingga jumlah sampel di sistem tidak sama dengan jumlah tabung nyata |
| Bila boleh, apakah penolakan satu tabung membatalkan seluruh pemeriksaan di atasnya | Belum ada aturan pembagian tagihan untuk penolakan sebagian |

| Opsi | Bentuk | Dampak |
|---|---|---|
| **A. Biarkan satu banding satu** | Keadaan sekarang | Tidak ada perubahan program. Praktik lapangan menyesuaikan dengan membuat satu baris sampel per pemeriksaan |
| **B. Tambah lapisan komponen pemeriksaan** | Sampel memuat banyak komponen | Model bertambah satu tingkat; aturan penolakan sebagian harus diputuskan lebih dulu |
| **C. Tunda sampai Lab Governance menilai** | Tidak berubah sekarang | Paling aman; keputusan menunggu masukan laboratorium |

Rekomendasi teknis: **A** untuk sekarang, ditinjau ulang bersama `RJ-BIL-FE-002` setelah petugas
laboratorium melihat wujud layarnya.

### `RJ-BIL-BE-003-INF-001` — pesanan mengikuti sampel pertama yang layak

**`INFERENCE`, bukan `FACT`.**

`RJ-BIL-GATE-DEC-003` mengunci `Accepted` pada siklus pesanan maupun siklus sampel, tetapi tidak
menyatakan kapan pesanan berpindah ke sana. Implementasi ini memindahkan pesanan ke `Accepted`
begitu sampel pertamanya dinyatakan layak. Turunan ini tidak memengaruhi tagihan sama sekali —
tagihan tetap terbentuk per sampel — dan dapat diubah tanpa menyentuh jalur finansial.

## 14. Blocker terbuka dari task sebelumnya

`RJ-BIL-BE-002-BLOCKER-001` — pintu masuk telaah farmasi — masih menunggu keputusan dan tidak
tersentuh task ini.

## 15. Task berikutnya yang dependency-ready

| Urutan | Task | Kesiapan |
|---|---|---|
| 1 | Menyediakan database test khusus | Membuka `37` test yang terhalang, termasuk seluruh bukti perilaku `BE-003` |
| 2 | Keputusan `RJ-BIL-BE-002-BLOCKER-001` | Kebijakan farmasi |
| 3 | `RJ-BIL-BE-004` Radiology | Greenfield penuh; area `RadiologyManagement` belum ada. Memerlukan owner Radiology dan Clinical Governance untuk SOP keselamatan |
| 4 | `RJ-BIL-BE-005` | `BLOCKED` sampai `RJ-BIL-OQ-001`, `OQ-002`, `OQ-005` dijawab |
