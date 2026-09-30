# Permintaan untuk Platform Authorization — Resource yang Dapat Diberikan Tanpa Endpoint

| Field | Nilai |
|---|---|
| Dari | Yasmin, owner / penggarap modul Finance Management (AR/AP), `FIN-BP-001` |
| Untuk | Security Owner bersama owner modul `platform-authorization` |
| Tanggal | 29 September 2026 |
| Sifat | **Permintaan perluasan mekanisme, menunggu jawaban.** Bukan laporan cacat, dan bukan permintaan mendesak — tidak ada pekerjaan yang tertahan karenanya |
| Dasar temuan | Pemeriksaan source read-only pada commit `7811c048` (`Attributes/AccessExplicitPermissionAttribute.cs`, `Services/Security/PermissionRegistryDescriptor.cs`, `Seeders/AccessMenuSeeder.cs`, `Areas/Administrator/Setting/Controllers/RoleAccessController.cs`) |
| Dasar keputusan sisi Finance | `FIN-DEC-082`, `FIN-DEC-083`, `FIN-DEC-084`; diturunkan ke `FIN-DES-066`..`069` (masih `draft`) |
| Dicatat sebagai | `FIN-OQ-039` pada decision log Finance (**TERBUKA**) |

Berkas ini berdiri sendiri. Anda tidak perlu membuka dokumen blueprint Finance untuk membacanya.

---

## 1. Ringkasan satu paragraf

Finance ingin menyediakan dua "resource payung" hak akses — satu untuk rumpun Hutang/Pengadaan, satu
untuk rumpun Piutang/Penagihan — supaya admin cukup mencentang satu kotak, alih-alih mencentang
sembilan (AP) atau empat (AR) resource granular satu per satu saat menyiapkan peran baru. Saat
merancangnya, kami menemukan bahwa mekanisme registry hari ini **tidak dapat menampung resource
seperti itu**: sebuah resource hanya dapat terdaftar bila ia ditemukan dari pemindaian endpoint,
sedangkan resource payung menurut sifatnya memang tidak menjaga endpoint apa pun. Kami **tidak**
menyiasatinya dari sisi Finance, dan kami juga **tidak** menahan pekerjaan Finance karenanya.
Bagian 4 mengajukan satu perluasan kecil pada mekanisme milik Anda, beserta satu syarat yang kami
minta dipertahankan supaya perluasan ini tidak melemahkan penjaga yang sudah ada.

---

## 2. Apa yang kami temukan di source `7811c048`

Kami menelusuri jalur pendaftaran identitas hak akses untuk memastikan rencana Finance tidak
bertabrakan dengan mekanisme milik Anda:

| Berkas | Yang kami baca | Kesimpulan |
|---|---|---|
| `Services/Security/PermissionRegistryDescriptor.cs` | Registry disusun dari **dua** sumber: pemindaian atribut `[AccessController]`/`[AccessAction]` pada controller nyata, dan `[assembly: AccessExplicitPermission(...)]` | Tidak ada sumber ketiga |
| `Attributes/AccessExplicitPermissionAttribute.cs` | Dokumentasi atributnya menyatakan `ResourceName` **wajib menunjuk resource yang sudah terdaftar dari pemindaian endpoint pada modul yang sama**, dan penanda yang menunjuk resource tak dikenal **ditolak keras** saat registry disusun | Jalur penanda dapat menambah **aksi** ke resource yang sudah ada, tetapi **tidak dapat melahirkan resource baru** |
| `Seeders/AccessMenuSeeder.cs` | Seeder hanya mengelola tiga tabel registry (`SysApplicationModule`, `SysControllerAccess`, `SysActionAccess`) dan komentar kelasnya menyatakan ia **tidak pernah** membuat `SysAccessPolicy` | Pemberian hak tetap sepenuhnya keputusan admin — dan itu memang yang kami inginkan |
| `Areas/Administrator/Setting/Controllers/RoleAccessController.cs` | `ApplyPoliciesAsync` adalah **satu-satunya** jalur penulisan `SysAccessPolicy`, dipakai bersama oleh endpoint simpan dan endpoint salin | Titik tulis yang kami butuhkan sudah terpusat rapi di satu tempat |

**Kesimpulan yang menghentikan rancangan kami:** resource payung yang tidak memiliki satu pun
endpoint **tidak dapat didaftarkan** dengan mekanisme hari ini. Ini bukan soal selera penamaan; ini
batas kemampuan yang nyata.

---

## 3. Kenapa Finance membutuhkannya — dan kenapa kami sampaikan juga alasan yang TIDAK berlaku

Kami merasa perlu menyampaikan keduanya, karena Anda sedang diminta mengubah kode milik modul Anda
dan berhak menimbang bobotnya secara jujur.

**Alasan yang berlaku.** Menyiapkan satu peran baru di rumpun Hutang/Pengadaan hari ini menuntut
admin mencentang aksi pada sembilan resource granular berbeda; rumpun Piutang menuntut empat. Satu
resource yang terlewat menghasilkan staf yang tertolak di satu layar saja — jenis kesalahan yang
sulit terlihat saat pemberian hak, dan baru ketahuan saat petugas bekerja.

**Alasan yang TIDAK berlaku, dan sebelumnya kami kira berlaku.** Rancangan awal kami beralasan bahwa
payung dibutuhkan supaya penyaring menu frontend tidak menampilkan butir menu yang endpointnya akan
menolak dengan `403`. Impact scan frontend (`a31da3c21`) membuktikan kekhawatiran itu **tidak
berdasar**: butir menu yang ada sekarang sudah konsisten dengan endpoint yang dipanggilnya, dan butir
menu yang kami khawatirkan belum pernah dibuat. Kami sudah menetapkan (`FIN-DES-069`) bahwa butir
menu memakai resource granular, **bukan** payung. Jadi manfaat payung yang tersisa **murni kemudahan
admin** — nyata, tetapi bukan soal keamanan, dan bukan keadaan darurat.

---

## 4. Yang kami minta

Satu perluasan pada jalur pendaftaran identitas: **sebuah modul dapat mendeklarasikan resource yang
dapat diberikan admin, tetapi tidak menjaga endpoint mana pun** — lewat deklarasi *opt-in* yang
eksplisit dan terbaca sebagai niat, bukan sebagai kelalaian.

| Aspek | Yang kami mintakan |
|---|---|
| Bentuk deklarasi | Sepenuhnya kewenangan Anda. Kami tidak mengusulkan nama atribut maupun bentuk API-nya |
| **Syarat yang kami minta DIPERTAHANKAN** | Penjaga yang ada sekarang — penanda yang menunjuk resource tak dikenal **ditolak keras** — **MUST tetap berlaku untuk kasus normal**. Yang kami minta adalah *jalan sah tambahan* untuk menyatakan "resource ini memang sengaja tanpa endpoint", bukan pelonggaran penjaga itu. Alasan penjaga itu ada (salah ketik menghasilkan `403` permanen yang tidak dapat diperbaiki dari layar mana pun) tetap valid sepenuhnya |
| Yang dibutuhkan Finance | Dua resource, masing-masing dengan **tepat tiga aksi**: `View`, `Operate`, `Approve` — dengan `AccessType` berturut-turut `Read`, `Create`, `Update`, supaya keduanya lolos penyaring `AccessTypes.AllowedForRoleAccess` dan tampil pada kolom yang benar di layar Akses Role |
| Yang **tidak** kami minta | Nol perubahan pada `AccessPermissionService.HasAccessAsync`. Pemeriksaan hak akses saat request tetap pencarian langsung atas `SysAccessPolicy` apa adanya. Nol tabel baru. Nol kolom baru |

**Satu catatan yang mungkin memudahkan penilaian Anda.** Dokumentasi `AccessExplicitPermissionAttribute`
sendiri menyebutkan bahwa kedua jalur penemuan — `Build(provider)` yang dipakai seeder dan
`BuildFromAssembly(assembly)` yang dipakai authorization verifier — bermuara pada `BuildCore` yang
sama. Bila perluasan ini diletakkan di sana, seeder dan verifier otomatis melihat hal yang sama,
sehingga tidak melahirkan selisih `DB_ONLY_ACTIVE` pada audit drift.

---

## 5. Alternatif yang sudah kami pertimbangkan, dan kenapa kami tidak menempuhnya

Kami tidak datang dengan satu pilihan saja. Dua jalan lain sudah kami timbang dan tolak — keduanya
dapat kami tempuh **tanpa** melibatkan modul Anda, jadi penolakan ini kami jelaskan supaya Anda tahu
permintaan ini bukan karena kami mencari jalan termudah.

| Alternatif | Kenapa ditolak |
|---|---|
| **Membuat controller pembawa di Finance** — satu controller baru yang memiliki resource payung beserta satu endpoint nyata, murni supaya resource-nya terdaftar | Endpoint yang keberadaannya terutama untuk membawa resource adalah jebakan jangka panjang: peninjau berikutnya wajar menganggapnya endpoint mati lalu menghapusnya, dan penghapusan itu **diam-diam mematikan seluruh pemberian hak lewat payung**. Kami menilai utang semacam ini tidak pantas ditanam di kode hanya untuk menghindari percakapan lintas modul |
| **Membatalkan payung sama sekali** | Sempat serius kami pertimbangkan justru setelah alasan `403` di bagian 3 terbukti gugur. Tidak dipilih karena kemudahan admin tetap dinilai bernilai oleh owner Finance. Bila Anda menilai perluasan ini tidak sepadan, **membatalkan payung adalah jawaban yang kami terima sepenuhnya** — lihat bagian 7 |

---

## 6. Yang kami lakukan di sisi Finance selama menunggu

**Tidak ada pekerjaan Finance yang tertahan permintaan ini.** Kami sampaikan ini supaya Anda dapat
menjadwalkannya tanpa tekanan waktu dari kami:

1. Penyelarasan nama enam resource hak akses Finance ke bentuk kanonikal (`FinancePayment`,
   `FinanceReceipt`, `FinanceReceivable`, `FinanceSupplierPayable`, `FinanceBillingIntake`,
   `FinanceAccountingEvent`) **sudah selesai** dan berdiri sendiri dari permintaan ini.
2. Migrasi data perannya sudah ditulis mengikuti pola `Migrations/scripts/be-sec-003b-policy-expansion.sql`
   milik Anda — dry-run baca-saja, dua tahap dalam dua transaksi terpisah, gerbang prasyarat,
   verifikasi parity yang ditinjau manusia, dan bagian rollback. Berkasnya
   `Migrations/scripts/be-fin-042-role-permissions-migration.sql`.
3. Butir menu frontend memakai resource granular, sehingga tidak bergantung pada payung sama sekali.

Mekanisme ekspansi payung sendiri **tidak akan kami implementasikan** sebelum jawaban Anda turun.

---

## 7. Yang kami butuhkan

Satu dari dua jawaban berikut, dan keduanya sama-sama dapat kami terima:

1. **Perluasan disetujui** — beserta bentuk deklarasi yang Anda tetapkan dan perkiraan penjadwalannya.
   Sesudah itu Finance menyusun task implementasi ekspansi, dengan titik tulis di
   `RoleAccessController.ApplyPoliciesAsync` yang **MUST ditinjau pemilik platform** sebelum digabung.
2. **Perluasan ditolak atau ditunda** — Finance akan mencabut rencana payung dan tetap memakai
   pemberian hak granular satu per satu. Tidak ada kerugian teknis; yang hilang hanya kemudahan admin.

Kami juga membuka kemungkinan jawaban ketiga bila Anda melihat jalan yang tidak terpikirkan kami.

---

## 8. Satu temuan kecil di luar permintaan ini, dilaporkan sekadar supaya tercatat

Bukan bagian dari permintaan, dan bukan milik modul Anda untuk diperbaiki — kami laporkan karena
ditemukan pada penelusuran yang sama dan menyangkut mekanisme hak akses.

Dua butir menu Finance (`Report AR` dan `Report AP`) dijaga pasangan hak akses `Finance.AR : Report`
dan `Finance.AP : Report`. Aksi `Report` **tidak pernah dideklarasikan** controller mana pun,
sehingga ia tidak pernah menjadi baris `SysActionAccess`, tidak dapat dicentang admin, dan tidak
pernah muncul pada daftar izin efektif. Karena penyaring menu bersifat *fail-closed*, kedua butir itu
**tersembunyi permanen bagi semua orang, termasuk SuperAdmin**. Endpoint laporannya sendiri sehat —
keduanya dijaga aksi `View` yang memang terdaftar. Kami catat sebagai `FIN-CQ-09` dan akan
menyelesaikannya di sisi Finance/frontend.
