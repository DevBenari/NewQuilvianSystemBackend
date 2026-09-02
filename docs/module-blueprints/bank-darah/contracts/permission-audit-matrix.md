# Bank Darah — Permission & Audit Matrix

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` · Contract version `v2` — `draft` |
| `last_changed_in` | `v2` |
| Owner | Pemilik keamanan platform · pemilik proses BDRS (peran) |
| `approved_by` / `approved_at` | Kosong — `draft` |
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
`Issue`, `EmergencyIssue`, `Correct`, `Resolve`, `Validate`, **`Store`**.

Action **`Store`** baru pada `v2`. Ia menjaga dua endpoint penyimpanan pada `BloodUnit`
(`POST`/`PUT /{id}/storage-location`) dan sengaja **tidak** disatukan dengan `Allocate`: menaruh kantong
ke kulkas adalah pekerjaan gudang, sedangkan mengalokasikan adalah mengikat kantong pada pasien. Rumah
sakit yang ingin memisahkan kedua tanggung jawab itu dapat melakukannya tanpa menunggu perubahan kode.

---

## 2. Peta peran rumah sakit → butir hak akses

Peran final menunggu `DEF-BD-004`; peta di bawah adalah **usulan** berbasis tanggung jawab aktor
(`00-interview-decisions.md` §3), belum disetujui.

| Peran | Resource : Action yang diusulkan | Catatan |
| --- | --- | --- |
| Unit pelayanan / dokter peminta | `BloodOrder : Create`, `Read` | Hanya unit `IsAvailableForBloodOrder=true` (dijaga aturan bisnis, bukan hak akses) |
| Petugas Bank Darah / BDRS | `BloodOrder : *`, `BloodProviderRequest : *`, `BloodUnit : Read/**Store**/Allocate/Compatibility/Issue`, `BloodGroupExam : Create/Update/Read`, `BloodBankProcedure : *` | Pelaksana alur normal. `Store` mencakup penetapan lokasi pertama **dan** perpindahan lokasi (`DEC-BD-036`, `DEC-BD-037`) |
| Peran berwenang jalur darurat | `BloodUnit : EmergencyIssue` | **`UNRESOLVED` `DEF-BD-004`** — kandidat Dokter BDRS |
| Peran validator golongan darah | `BloodGroupExam : Validate` | **`UNRESOLVED` `DEF-BD-004`** |
| Peran pencatat koreksi pemberian | `BloodUnit : Correct` | **`UNRESOLVED` `DEF-BD-004`** |
| Peran penyelesai kantong `PendingReview` | `BloodUnit : Resolve` | Perannya `UNRESOLVED`; bentuknya `DEC-BD-019` |
| Admin master data Bank Darah | `BloodComponent : *`, `BloodBankReason : *`, **`BloodStorageLocation : *`** | Setup MVP — kini **tiga** master setelah amandemen `DEC-BD-024` oleh `DEC-BD-035` |

Pembatalan alokasi (`BloodUnit : Allocate` pada `cancel-allocation`) **tidak** menunggu `DEF-BD-004`:
`DEC-BD-029` menyatakannya kekeliruan administratif biasa, cukup petugas Bank Darah.

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
| Pemberian jalur darurat | Semua di atas + penanda permanen + **keterangan gerbang mana yang dilewati** (bukti kecocokan, lokasi nonaktif, atau keduanya — `INV-BD-030`) + alasan |
| Pengalihan kantong | Pasien asal → alasan pelepasan → pasien tujuan (rantai tak putus) + bukti mana yang gugur |
| Koreksi pemberian | Pemberian asal, apa yang keliru, apa yang benar, alasan, pelaku, waktu — asal tak berubah |
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
