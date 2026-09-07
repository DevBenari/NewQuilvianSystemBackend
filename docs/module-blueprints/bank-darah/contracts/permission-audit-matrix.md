# Bank Darah — Permission & Audit Matrix

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` · Contract version `v4` — **`approved`** |
| `last_changed_in` | `v4` |
| Owner | Pemilik keamanan platform · pemilik proses BDRS (peran) |
| `approved_by` / `approved_at` | `Sukmagp` / `2026-09-03` |
| Sumber | `contracts/api-contract.md` (pemetaan endpoint) · `data/data-dictionary.md` (kolom sensitif) · `BD-CAP-013` |

Dokumen ini **tidak** mendaftar ulang endpoint. Pemetaan endpoint→hak akses hanya hidup di kolom
"Hak akses" pada `api-contract.md`. Dua turunan berikut **dihitung**, bukan ditulis ulang:

- String `[AccessPermission("<Resource>", "<Action>")]` = disalin dari kolom "Hak akses".
- Status pencatatan logger = konvensi project: `GET` tidak dicatat, selain `GET` dicatat.

Yang ditulis di sini justru bagian yang tidak dapat diturunkan dari daftar endpoint.

---

## 1. Cara kerja hak akses di repository ini

Bank Darah **memakai** model keamanan yang sudah ada; tidak ada model baru (`BD-CAP-013`).

| Lapisan | Mekanisme | Rujukan source |
| --- | --- | --- |
| Penanda controller | `[AccessController]` di kelas | `Attributes/AccessControllerAttribute.cs@9522caa` |
| Penanda tindakan | `[AccessAction]` di endpoint | `Attributes/AccessActionAttribute.cs@9522caa` |
| Hak akses | `[AccessPermission("Resource", "Action")]` di endpoint | `Attributes/AccessPermissionAttribute.cs@9522caa` |
| Autentikasi | `[Authorize]` tingkat kelas | contoh `LabOrderController.cs@9522caa` |
| Pendaftaran resource/action | Seeder hak akses platform | mengikuti pola resource existing (mis. `LabOrder`) |

Resource baru yang perlu didaftarkan seeder: `BloodOrder`, `BloodProviderRequest`, `BloodUnit`,
`BloodGroupExam`, `BloodBankProcedure`, `BloodComponent`, `BloodBankReason`, **`BloodStorageLocation`**.
Action yang dipakai: `Read`, `Create`, `Update`, `Delete`, `Process`, `Allocate`, `Compatibility`,
`Issue`, `EmergencyIssue`, `Correct`, `Validate`, **`Store`**, **`ResolveConflict`**,
**`ApproveCorrection`**, **`ResolveReallocate`**, **`ResolveReturn`**, **`ResolveNotUsable`**,
**`Cancel`**.

Action `Resolve` **tidak lagi dipakai** sejak `v4`; ia digantikan tiga butir penyelesaian yang
terpisah (`DEC-BD-043`). Seeder **MUST NOT** mendaftarkannya, supaya tidak ada jalan pintas yang
membatalkan pemisahan itu.

Dua butir baru pada `v3`, keduanya turunan penutupan `DEF-BD-004`:

| Butir | Menjaga | Sebabnya terpisah |
| --- | --- | --- |
| `BloodGroupExam : ResolveConflict` | Penyelesaian konflik hasil golongan darah | Dipisah dari `Validate` oleh `DEC-BD-039`. Satu butir untuk keduanya membuat siapa pun yang boleh memvalidasi hasil rutin otomatis boleh menutup konflik |
| `BloodUnit : ApproveCorrection` | Menyetujui atau menolak koreksi pencatatan | Dipisah dari `Correct` oleh `DEC-BD-041`. `Correct` kini hanya berarti **mengajukan** |

Otorisasi darurat **tidak** mendapat butir baru: `BloodUnit : EmergencyIssue` yang sudah ada tetap
dipakai, dan yang bertambah hanya isi rekamnya (`DEC-BD-040`).

Empat butir lagi pada `v4`, seluruhnya turunan penutupan sisa `DEF-BD-004`:

| Butir | Menggantikan | Menjaga |
| --- | --- | --- |
| `BloodUnit : ResolveReallocate` | `BloodUnit : Resolve` | Pengalihan kantong `PendingReview` ke pasien lain |
| `BloodUnit : ResolveReturn` | `BloodUnit : Resolve` | Pengembalian kantong ke PMI |
| `BloodUnit : ResolveNotUsable` | `BloodUnit : Resolve` | Penetapan kantong tidak layak |
| `BloodOrder : Cancel` | dipisah dari `BloodOrder : Update` | Pembatalan order darah |

**Butir `BloodUnit : Resolve` dihapus, bukan disisakan sebagai payung.** Membiarkannya hidup
berdampingan dengan ketiga penggantinya akan menciptakan jalan pintas: siapa pun yang memegang `Resolve`
lama tetap dapat mengalihkan kantong, sehingga pemisahan yang justru dituju `DEC-BD-043` batal dengan
sendirinya.

**`BloodOrder : Cancel` dipisah dari `Update` supaya dapat diberikan sendirian.** Dokter peminta perlu
membatalkan ordernya tanpa ikut memperoleh wewenang menyunting order secara umum. Satu butir dipakai
kedua peran; yang membedakan sebabnya adalah **kategori alasan**, bukan penjaga yang berbeda
(`DEC-BD-044`).

Action **`Store`** baru pada `v2`. Ia menjaga dua endpoint penyimpanan pada `BloodUnit`
(`POST`/`PUT /{id}/storage-location`) dan sengaja **tidak** disatukan dengan `Allocate`: menaruh kantong
ke kulkas adalah pekerjaan gudang, sedangkan mengalokasikan adalah mengikat kantong pada pasien. Rumah
sakit yang ingin memisahkan kedua tanggung jawab itu dapat melakukannya tanpa menunggu perubahan kode.

---

## 2. Peta peran rumah sakit → butir hak akses

`DEF-BD-004` sudah ditutup oleh `DEC-BD-039`, `DEC-BD-040`, dan `DEC-BD-041` untuk **tiga** wewenang:
validator golongan darah, jalur darurat, dan koreksi pencatatan. Baris-baris itu kini menurunkan
keputusan yang sudah diambil pemilik proses, bukan usulan. Ketiganya masih berstatus `draft` pada
register keputusan, seperti seluruh keputusan Bank Darah lainnya — approval `G1` 3 September 2026
menutup **blueprint dan set kontrak `v4`**, dan tidak menaikkan status register keputusan.

**`DEF-BD-004` kini tertutup seluruhnya.** `03-domain-architecture.md` §H membawa **enam** wewenang
sebagai satu keputusan terkumpul; role & authority closure pass menjawab tiga, dan role residue closure
pass menjawab tiga sisanya (`DEC-BD-042`, `DEC-BD-043`, `DEC-BD-044`). Tidak ada baris peran yang masih
`UNRESOLVED`.

Satu hal yang sempat belum bernama — pemegang `BloodUnit : ResolveNotUsable` — **kini sudah
ditetapkan**. `DEC-BD-043` menyebutnya "mengikuti kewenangan penetapan kelayakan sesuai proses BDRS",
sebuah penunjuk yang belum dapat dipetakan ke seeder. `DEC-BD-045` mengisinya pada 3 September 2026:
butir itu dipegang **kewenangan operasional BDRS**, peran yang sama dengan pemegang
`BloodUnit : ResolveReturn`. Dasarnya diambil dari `DEC-BD-043` sendiri — tabel sifat tindakannya sudah
menandai pengembalian ke PMI dan penetapan tidak layak dengan kalimat yang sama, "mengeluarkan darah
dari peredaran".

⚠️ **Peran yang sama tidak berarti butir yang sama, dan seeder wajib membacanya begitu.**
`INV-BD-034` tetap menuntut tiga butir terpisah. Peran operasional BDRS menerima `ResolveReturn` **dan**
`ResolveNotUsable` sebagai **dua baris seeder**, bukan satu butir gabungan. Dua alasannya: jaminan
`AC-BD-093` — pemegang kewenangan operasional tetap ditolak saat mencoba mengalihkan kantong ke pasien
lain — hanya bekerja bila butirnya terpisah; dan pencabutan kewenangan pembuangan tanpa ikut mencabut
kewenangan pengembalian ke PMI hanya mungkin bila keduanya berdiri sendiri. Menggabungkannya akan
membatalkan pemisahan `DEC-BD-043` lewat pintu belakang.

Satu hal yang perlu dibaca apa adanya: Quilvian **hanya menerapkan hak akses dan mencatat audit**. Ia
tidak menilai kompetensi klinis siapa pun. Penempatan orang pada peran adalah tanggung jawab rumah
sakit lewat pengelolaan role platform.

| Peran | Resource : Action yang diusulkan | Catatan |
| --- | --- | --- |
| Unit pelayanan / dokter peminta | `BloodOrder : Create`, `Read` | Hanya unit `IsAvailableForBloodOrder=true` (dijaga aturan bisnis, bukan hak akses) |
| Petugas Bank Darah / BDRS | `BloodOrder : *`, `BloodProviderRequest : *`, `BloodUnit : Read/**Store**/Allocate/Issue`, `BloodGroupExam : Create/Update/Read`, `BloodBankProcedure : *` | Pelaksana alur normal. `Store` mencakup penetapan lokasi pertama **dan** perpindahan lokasi (`DEC-BD-036`, `DEC-BD-037`). **`Compatibility` dicabut dari baris ini** oleh `DEC-BD-047` — lihat peringatan di bawah |
| **Dokter BDRS / penanggung jawab klinis** | `BloodGroupExam : ResolveConflict`, `BloodUnit : EmergencyIssue`, `BloodUnit : ApproveCorrection` | **Ditetapkan `DEC-BD-039`, `DEC-BD-040`, `DEC-BD-041`.** Tiga wewenang paling berat di modul ini |
| **DPJP pasien** | `BloodUnit : EmergencyIssue` | **Ditetapkan `DEC-BD-040`.** Rekam menyimpan peran yang dipakai, sehingga jalur wewenangnya terbaca saat audit |
| **Petugas BDRS berwenang validasi** | `BloodGroupExam : Validate` | **Ditetapkan `DEC-BD-039`.** Validasi hasil rutin saja; tidak mencakup penyelesaian konflik |
| **Petugas BDRS pengaju koreksi** | `BloodUnit : Correct` | **Ditetapkan `DEC-BD-041`.** Hanya mengajukan; keputusannya milik Dokter BDRS |
| **Petugas BDRS berwenang validasi** (butir kedua) | `BloodUnit : Compatibility` | **Ditetapkan `DEC-BD-042`, dipertegas `DEC-BD-047` sebagai satu-satunya pemegang.** Tingkat kewenangan yang sama dengan validasi golongan darah rutin. Pelaksana pemeriksaan **boleh** orang lain — izin, bukan kewajiban |
| **Pemegang kewenangan klinis BDRS** | `BloodUnit : ResolveReallocate` | **Ditetapkan `DEC-BD-043`.** Pengalihan memasukkan darah ke tubuh pasien baru |
| **Pemegang kewenangan operasional BDRS** | `BloodUnit : ResolveReturn` | **Ditetapkan `DEC-BD-043`.** Mengeluarkan darah dari peredaran — arah yang aman dengan sendirinya |
| **Pemegang kewenangan operasional BDRS** (butir kedua) | `BloodUnit : ResolveNotUsable` | **Ditetapkan `DEC-BD-043` (bentuk) dan `DEC-BD-045` (peran).** Peran yang sama dengan `ResolveReturn`, tetapi **butir seeder yang terpisah** — lihat peringatan di atas. Mengeluarkan darah dari peredaran |
| **Dokter peminta** | `BloodOrder : Cancel` | **Ditetapkan `DEC-BD-044`.** Alasan berkategori pembatalan klinis |
| **Petugas BDRS** (butir pembatalan) | `BloodOrder : Cancel` | **Ditetapkan `DEC-BD-044`.** Alasan berkategori pembatalan operasional |
| Admin master data Bank Darah | `BloodComponent : *`, `BloodBankReason : *`, **`BloodStorageLocation : *`** | Setup MVP — kini **tiga** master setelah amandemen `DEC-BD-024` oleh `DEC-BD-035` |

⚠️ **`BloodUnit : Compatibility` hanya milik petugas berwenang validasi, dan tidak boleh dikembalikan
ke baris peran umum.** Butir ini sempat tercantum pada baris Petugas Bank Darah / BDRS, bertentangan
dengan `DEC-BD-042`, `VAL-BD-078`, dan `AC-BD-090` yang ketiganya membatasinya pada petugas berwenang
validasi. Pertentangan itu ditemukan `BE-BD-016` dan ditutup `DEC-BD-047` pada 3 September 2026.

Kenapa ini perlu ditulis tebal, bukan sekadar dibetulkan diam-diam: **hak akses diperiksa lebih dulu
daripada aturan bisnis.** Bila butir ini kembali diberikan kepada seluruh petugas BDRS, `VAL-BD-078`
tidak akan pernah menyala — bukan karena aturannya dicabut, melainkan karena setiap pelaku sudah lolos
pemeriksaan hak akses sebelum aturan itu sempat memeriksa. Pembatasan `DEC-BD-042` batal di tingkat
seeder tanpa satu pun dokumen menyatakannya dicabut, dan akibatnya menyentuh keselamatan pasien: darah
dapat dinyatakan cocok oleh petugas yang tidak ditunjuk memvalidasi.

Yang **tidak** ikut berubah: kelonggaran `DEC-BD-042` tetap utuh. Pelaksana pemeriksaan boleh berbeda
dari validator, tetapi tidak diwajibkan — petugas berwenang validasi yang mengerjakan ujinya sendiri
tetap boleh menyatakannya sendiri (`AC-BD-091`).

Pembatalan alokasi (`BloodUnit : Allocate` pada `cancel-allocation`) **tidak** menunggu `DEF-BD-004`:
`DEC-BD-029` menyatakannya kekeliruan administratif biasa, cukup petugas Bank Darah.

**Pemisahan wewenang yang dijaga aturan bisnis, bukan hak akses.** Satu aturan pada `v3` sengaja
**tidak** ditaruh di mesin hak akses: peminta koreksi tidak boleh menyetujui permintaannya sendiri.
Alasannya, seseorang dapat sah memegang `BloodUnit : Correct` **dan** `BloodUnit : ApproveCorrection`
sekaligus — misalnya Dokter BDRS yang menemukan kekeliruan itu sendiri. Mesin hak akses akan
meloloskannya, dan memang seharusnya begitu; yang menahan adalah perbandingan pelaku di lapisan aturan
bisnis (`VAL-BD-073`). Ini contoh langsung dari kategori "kewenangan yang tidak dapat dijaga mesin hak
akses" pada bagian berikutnya.

Ketiga butir penyimpanan pada `v2` juga **tidak** menunggu `DEF-BD-004`. `DEC-BD-036` menyebut
pelakunya "petugas" tanpa syarat tambahan, dan `DEC-BD-009` sudah menetapkan penerimaan, alokasi, serta
pemberian dijalankan petugas Bank Darah tanpa gerbang persetujuan. Pengelolaan masternya mengikuti pola
Setup yang sudah ada. Tidak ada peran baru yang diciptakan, dan tidak ada peran yang diasumsikan.

**Satu batas wewenang yang sengaja dipisah.** Menonaktifkan lokasi (`BloodStorageLocation : Update`) dan
memindahkan kantong (`BloodUnit : Store`) adalah dua butir hak akses yang berbeda, dan `DEC-BD-037`
memang memisahkannya: pengelola Setup boleh menandai sebuah kulkas tidak layak pakai, tetapi ia
**tidak** dengan itu memerintahkan kantong berpindah. Perpindahan tetap tindakan tersendiri oleh
petugas BDRS, dengan pelaku dan waktunya sendiri pada riwayat. Sistem tidak pernah menjadi pelaku
perpindahan, sehingga tidak pernah ada baris riwayat penempatan yang pelakunya bukan manusia.

---

## 3. Kewenangan yang **tidak** dapat dijaga mesin hak akses

Hak akses menjaga "peran X boleh memanggil endpoint Y". Aturan berikut ada di tingkat aturan bisnis
(service), dan bila lolos dari sana, hak akses **tidak** menangkapnya:

| Aturan bisnis | Dijaga oleh | Risiko bila hilang |
| --- | --- | --- |
| Satu kantong ≤ satu alokasi aktif | Token konkurensi `Version` + validasi service (`VAL-BD-018c`) | Satu kantong diberikan ke dua pasien |
| Gerbang pemberian (bukti berlaku utk pasien tujuan, belum lewat masa berlaku) | Service (`VAL-BD-018/019/020`) | Darah diberikan tanpa bukti kecocokan yang sah — keselamatan |
| Pasien `IsConflictHeld` ditahan | Service (`VAL-BD-034`) | Darah diberikan atas golongan darah yang bertentangan |
| Sisa permintaan tak negatif | Service token `Version` (`BD-XINV-03`) | Angka pemenuhan PMI menyesatkan |
| Alasan wajib dari daftar terkendali | Service (`VAL-BD-016`) | Riwayat tak dapat dianalisis; koreksi jadi teks bebas |
| Unit `IsAvailableForBloodOrder` | Service (`VAL-BD-013`) | Unit tak berwenang membuat order |

---

## 4. Audit — kejadian yang wajib meninggalkan jejak tahan lama

Mengikuti pola `BD-CAP-009` (`BbkTransitionHistory`, append-only) dan kolom audit `IdentityModel`
(`BD-CAP-011`).

| Kejadian | Yang wajib tersimpan |
| --- | --- |
| Perpindahan status order/permintaan/kantong | Status sebelum & sesudah, pelaku, waktu, korelasi pemicu |
| Pembatalan (order/permintaan/alokasi) & penyelesaian kantong | Kode alasan **beserta salinan teksnya saat kejadian** |
| Pemberian darah | Pelaku, waktu, kantong, pasien, order, rujukan bukti kecocokan |
| Pemberian jalur darurat | Semua di atas + penanda permanen + **keterangan gerbang mana yang dilewati** (`INV-BD-030`) + alasan terkendali + **keterangan kondisi kedaruratan** + **peran yang dipakai penerbit** (`INV-BD-032`) |
| Pengalihan kantong | Pasien asal → alasan pelepasan → pasien tujuan (rantai tak putus) + bukti mana yang gugur |
| Pengajuan koreksi pemberian | Pemberian asal, apa yang keliru, apa yang benar, alasan terkendali, bukti pendukung, **peminta**, waktu pengajuan — asal tak berubah |
| Keputusan atas koreksi | **Pemutus** (wajib berbeda dari peminta), waktu keputusan, hasil keputusan, dan alasan bila ditolak. Permintaan yang ditolak **tetap tersimpan** (`DEC-BD-041`) |
| Deteksi & penyelesaian konflik golongan darah | Hasil-hasil bertentangan, sejak kapan tertahan, validator, pemeriksaan ulang yang memutus, alasan, waktu |
| **Penetapan lokasi penyimpanan pertama** | Lokasi yang dipilih, pelaku, waktu, dan perpindahan kantong dari `Received` ke `Stored` |
| **Perpindahan lokasi penyimpanan** | Lokasi asal, lokasi tujuan, pelaku, waktu. **Pelakunya selalu manusia**, tidak pernah sistem (`DEC-BD-037`). Status kantong dan catatan penerimaan awalnya tidak berubah (`INV-BD-026`) |
| **Penonaktifan / pengaktifan lokasi penyimpanan** | Pelaku, waktu, dan **salinan nama lokasi saat kejadian**, supaya penempatan lama tetap terbaca walaupun lokasinya kelak berganti nama |
| **Penolakan alokasi atau pemberian karena lokasi nonaktif** | Kantong, pasien tujuan, lokasi yang menutup gerbang, pelaku yang mencoba, dan waktu. Penolakan yang tidak terbaca membuat petugas menyangka sistem rusak, bukan menyangka ada kulkas bermasalah |
| Perubahan master komponen & alasan | Pelaku & waktu |

Logger custom mencatat hanya `EntityId`, controller, action, status. **MUST NOT** memuat diagnosis,
keluhan, nomor kantong, atau data medis/pribadi.

Nama dan kode lokasi penyimpanan **bukan** data sensitif — keduanya nama perabot, bukan keterangan
tentang pasien — sehingga boleh muncul pada pesan penolakan `VAL-BD-060`, `VAL-BD-064`, dan
`VAL-BD-065` supaya petugas tahu kulkas mana yang bermasalah.

---

## 5. Kolom sensitif dan masa simpan

| Kolom / data | Tabel | Perlakuan |
| --- | --- | --- |
| `PmiBagNumber` | `BbkBloodUnit` | **Sensitif** — dari PMI, tak dijamin bebas keterangan pribadi. **MUST NOT** masuk payload log; jangan jadi alat otorisasi |
| `SampleIdentifier` | `BbkBloodGroupSample` | **Sensitif** — identifier internal tanpa data pribadi (pola `BD-CAP-008`) |
| `PatientId` (dan seluruh rujukan pasien) | banyak | Rujukan; response pasien mengikuti kebijakan masking PatientManagement |
| `ReasonNote` | riwayat/koreksi | Dapat memuat konteks; **MUST NOT** masuk log; tinjau masking pada response |
| `AboRhesusResult` | `BbkBloodGroupExam` | Data klinis — **MUST NOT** masuk log |

Masa simpan mengikuti kebijakan retensi rekam medis rumah sakit; sifat append-only (`IsDelete` sebagai
penandaan, bukan hapus keras — `BD-CAP-011`) menjaga jejak klinis tetap dapat ditelusuri.

### Pengecualian bernama dari konvensi logger

Tidak ada. Seluruh endpoint non-`GET` dicatat sesuai konvensi; tidak ada endpoint `GET` yang perlu
dicatat khusus.
