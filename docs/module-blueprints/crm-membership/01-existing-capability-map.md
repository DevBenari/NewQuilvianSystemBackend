# CRM Membership — Peta Kemampuan yang Sudah Ada

| Field | Nilai |
|---|---|
| Blueprint ID | `crm-membership` |
| Revision | `1` |
| Status | `draft` |
| Masukan | `00-interview-decisions.md` revision 1 (scope pass 2026-10-13, `CRM-DEC-001` s.d. `CRM-DEC-016`) |
| Backend SHA | `36422a4d` (branch `yoga`, working tree bersih kecuali folder blueprint ini) |
| Frontend SHA | `9aba9929d` |
| Contract version | Belum ada (blueprint belum punya kontrak target) |
| Tanggal audit | 2026-10-13 |
| Sifat | Read-only. Tidak ada source, database, atau runtime yang diubah atau dijalankan |

## Batas Audit

Audit hanya mencakup kemampuan yang disentuh 16 keputusan scope pass:

1. Master tingkat keanggotaan.
2. Data keanggotaan per pasien (siapa member, tingkat apa, kapan berlaku).
3. Pencarian pasien lewat No. RM, NIK, dan No. Telp.
4. Hak akses (izin per departemen-posisi).
5. Penagihan iuran tanpa kunjungan (`CRM-OQ-005`).
6. Titik sentuh diskon, antrean prioritas, dan cara bayar membership di billing.

Yang **tidak** diaudit: alur reservasi RJ, antrean kelipatan 5, IGD, dan rawat inap. Semuanya
di luar scope (`CRM-DEC-001`). Runtime dan database tidak diperiksa, sehingga isi data nyata
(misalnya berapa baris membership di DB dev) berstatus `Unknown`.

## Ringkasan untuk Pembaca Umum

- **Sudah ada lebih banyak dari dugaan awal.** Sistem sudah punya tabel membership per pasien
  (`MstPatientMembership`) beserta halaman daftar, tambah, ubah, dan detail di frontend. Fakta
  `CRM-FACT-005` di decision log ("belum ditemukan") **keliru** dan sudah dikoreksi.
- **Namun perilakunya belum sesuai keputusan.** Sistem sekarang memperlakukan membership
  sebagai data isian bebas. Petugas mengetik sendiri status, tanggal berlaku, dan nomor member.
  Tidak ada aturan "aktif setelah lunas", tidak ada kedaluwarsa otomatis, dan satu pasien boleh
  punya beberapa membership aktif sekaligus.
- **Kasir belum bisa menagih iuran.** Setiap tagihan kasir wajib terikat pada satu kunjungan
  pasien. Iuran membership tidak punya kunjungan, sehingga belum ada wadahnya.
- **Diskon member belum dipakai billing.** Persen diskon per tingkat sudah tersimpan di master,
  tetapi tidak ada kode billing yang membacanya.

## Peta Kemampuan

| ID | Kebutuhan | Pemilik | Bukti (`repo/path#symbol@SHA`) | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `CRM-CAP-001` | Master tingkat (Silver/Gold/Platinum/Executive) dengan persen diskon dan masa berlaku | Administrator / Master Data | BE `Areas/Administrator/MasterData/Models/MstMembershipTier.cs#MstMembershipTier@36422a4d`. Controller `MembershipTierController.cs:26,36`. FE `src/app/administrator/master-data/membership-tiers/` dan menu "Tingkat Keanggotaan" (`src/utils/menu-sidebar/menu-items.jsx:117`) @`9aba9929d` | Reuse with adapter | Belum ada kolom **nominal iuran** (`CRM-DEC-004`). Enum `MembershipTierType` memuat Corporate/Family/Other yang tidak dipakai (`CRM-DEC-006`), sehingga pilihan saat pendaftaran perlu dibatasi | Rendah |
| `CRM-CAP-002` | Data membership per pasien | PatientManagement (`Pat`/`Mst`) | BE `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatientMembership.cs#MstPatientMembership@36422a4d`, kolom `PatientId`, `MembershipTierId`, `MemberNumber`, `MembershipStatus`, `JoinDate`, `ExpiredDate`, `IsPrimary`, `IsActive`, `IsCreatedByMarketing` | Extend | Tidak ada status **Menunggu Pembayaran** di enum `MembershipStatus`. Tidak ada tautan ke tagihan iuran. Tidak ada alasan pembatalan | Sedang |
| `CRM-CAP-003` | Aturan siklus hidup: aktif setelah lunas, batal otomatis 7 hari, kedaluwarsa otomatis, upgrade menutup yang lama, pembatalan wajib alasan | PatientManagement | `PatientMembershipController.cs#CreatePatientMembership` (baris 377), `#UpdatePatientMembership` (506), `#UpdatePatientMembershipStatus` (648), `#ValidateRequestAsync` (930) | Conflict | Lihat `CRM-CONF-003` s.d. `CRM-CONF-006`. Status, tanggal, dan nomor member diketik bebas lewat request. Tidak ada proses terjadwal untuk kedaluwarsa atau batas bayar | Tinggi |
| `CRM-CAP-004` | Status member pasien yang dibaca layanan lain | PatientManagement | `MstPatient.cs:84-88` (`IsMember`, `DefaultMembershipTierId`, `ActivePatientMembershipId`). Diisi oleh `PatientMembershipController#SyncPatientMembershipReferenceAsync` / `#RecalculatePatientMembershipReferenceAsync` (1065, 1087). Dibaca `NurseStationQueueController.cs:1234-1236`, FE `PatientInformationTab.jsx:50-57`, `emergency-registration.utils.js:923-926` | Repair | Penentu "member aktif" hanya `IsActive && IsPrimary`, tanpa melihat `ExpiredDate` maupun `MembershipStatus`. `IsMember` juga bisa diketik langsung saat membuat/mengubah pasien (`PatientController.cs:546,848`), dan default model `true` (`MstPatient.cs:84`) | Tinggi |
| `CRM-CAP-005` | Pencarian pasien lewat No. RM / NIK / No. Telp yang menampilkan semua hasil | PatientManagement | `PatientController.cs:1987-1992`: kata kunci dicocokkan ke `MedicalRecordNumber`, `IdentityNumber`, `PhoneNumber` dengan `Contains` | Reuse with adapter | Pencarian berbentuk "mengandung", bukan "sama persis", sehingga semua pasien bernomor serupa ikut tampil. Sudah memenuhi `CRM-DEC-010` (tampilkan semua, pilih satu). Status member perlu ikut ditampilkan di hasil | Rendah |
| `CRM-CAP-006` | Hak akses: hanya CRM/Marketing yang boleh mendaftar, upgrade, membatalkan | Platform Authorization | `PatientMembershipController.cs:365-377`: `POST /` dan `POST /kiosk` hanya memakai policy `KioskRead`. `POST /admin` memakai `[AccessPermission("PatientMembership","Create")]` (485-486). `KioskRead` = role SuperAdmin/Administrator/Kiosk atau klaim perangkat kiosk (`Program.cs:1083-1100`) | Conflict | Lihat `CRM-CONF-007` | Tinggi |
| `CRM-CAP-007` | Halaman Pendaftaran Membership dan Daftar Membership | Frontend PatientManagement | FE `src/app/health-services/patient-management/master-data/patient-memberships/` (daftar, create, detail, update). Slice `master-data-patient-membership-slice.jsx:4`. Masuk dari detail pasien (`use-master-data-patient-detail.jsx:402`) | Extend | **Tidak ada di menu sidebar** dan tidak ada bagian "CRM" (`CRM-FE-001`). Formulir memuat isian yang menurut keputusan seharusnya diisi sistem: `membershipStatus`, `joinDate`, `expiredDate`, `pointBalance`, `totalSpendAmount` (`patient-membership-editor-utils.jsx`) | Sedang |
| `CRM-CAP-008` | Tagihan iuran membership tanpa kunjungan (`CRM-OQ-005`) | Billing-Kasir | `Areas/HealthServices/BillingManagement/Billing/Models/BilInvoice.cs:11`: `EncounterId` wajib (`Guid`, bukan nullable). `FinNonPatientReceivable` hanya untuk sewa parkir/tenant dan sengaja dipisah dari billing (`FinNonPatientReceivable.cs:1-25,61-65`) | Missing | Kasir tidak punya wadah tagihan selain per kunjungan. Perlu amendment `billing-kasir` | Tinggi |
| `CRM-CAP-009` | Diskon per tingkat diterapkan pada kunjungan pribadi | Billing-Kasir | Kolom `*DiscountPercent` di `MstMembershipTier` tidak dibaca kode mana pun di luar master (`rg` tanpa hasil). Diskon billing (`BillingDiscountService.cs#ApplyAsync`) hanya mengenal `PROMO_TOTAL`, `PROMO_ITEM`, `DOCTOR` (`DiscountPolicyDtos.cs:7-17`) dan diterapkan manual pada invoice | Missing | Perlu amendment `billing-kasir`. Catatan: diskon billing saat ini diterapkan pada invoice, padahal `CRM-DEC-008` menuntut potongan sebelum pembagian penjamin. Urutan ini perlu dipastikan di modul billing | Sedang |
| `CRM-CAP-010` | Membership bukan cara bayar | Billing-Kasir | `BillingSettlementService.cs:1178-1187`: metode bayar `IsMembership` ditolak sebagai tender pasien ("Metode penjamin tidak dapat digunakan sebagai tender pasien") | Ready to reuse | Sudah sejalan dengan `CRM-DEC-008`. Tidak perlu diubah | Rendah |
| `CRM-CAP-011` | Antrean prioritas untuk member | RegistrationManagement (titik sentuh) | `TrxQueue.cs:103` `IsPriorityQueue` dibaca untuk urutan (`NurseStationQueueController.cs:607`, `QueueDisplayRuntimeController.cs:154`), tetapi **tidak ada kode yang mengisinya `true`**. `MstMembershipTier.PriorityQueue` tidak dibaca di luar master | Missing | Di luar scope CRM, diteruskan ke amendment `rawat-jalan`/`kiosk`. CRM cukup menyediakan status member yang benar (`CRM-CAP-004`) | Sedang |
| `CRM-CAP-012` | Pencarian kiosk lewat nomor member | RegistrationManagement | `KioskPatientLookupService.cs:195-207` mencocokkan `MemberNumber` pada membership `IsActive` | Reuse with adapter | Ikut membaca membership yang sudah lewat tanggal berlaku (masalah sama dengan `CRM-CAP-004`) | Rendah |
| `CRM-CAP-013` | Registrasi modul pemilik | Engineering governance | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md:24`: `PatientManagement operational`, prefix `Pat`, `ACTIVE`. Tidak ada entri CRM | Unknown | Bila desain membuat entity baru di bawah folder CRM baru, registrasi prefix wajib lebih dulu (`QBE-MOD-002`/`QBE-MOD-003`). Bila memperluas `MstPatientMembership` di PatientManagement, registrasi baru tidak diperlukan. Ini keputusan desain | Sedang |
| `CRM-CAP-014` | Uji otomatis | — | Tidak ada project test yang menyebut `PatientMembership` (`Tests/` hanya Pharmacy, OperatingRoom, Nutrition, dan lain-lain) | Missing | Aturan siklus hidup baru butuh uji | Sedang |
| `CRM-CAP-015` | Isi data nyata tabel membership dan tier di DB dev | — | Tidak diperiksa (tanpa izin eksekusi DB) | Unknown | Menentukan apakah perubahan aturan perlu pembersihan data lama | Sedang |

## Conflict

| ID | Keputusan | Perilaku source saat ini | Contoh dampak |
|---|---|---|---|
| `CRM-CONF-003` | `CRM-DEC-004`: aktif hanya setelah iuran lunas | `CreatePatientMembership` menerima `MembershipStatus` dari request, dengan default `Active` (`PatientMembershipDtos.cs:160`). Membership langsung terbaca aktif saat disimpan | Petugas menyimpan Gold pukul 09.00, dan pasien langsung mendapat perlakuan Gold walaupun belum membayar |
| `CRM-CONF-004` | `CRM-DEC-005`: kedaluwarsa otomatis menjadi Reguler | `ExpiredDate` hanya disimpan. Penentu status (`RecalculatePatientMembershipReferenceAsync`) tidak memeriksanya, dan tidak ada proses terjadwal | Gold berakhir 12 Okt 2027, tetapi pada 1 Jan 2028 pasien masih terbaca Gold di nurse station dan kiosk |
| `CRM-CONF-005` | `CRM-DEC-012`: satu membership aktif per pasien; upgrade menutup yang lama | Validasi hanya melarang **tingkat yang sama** dua kali (`ValidateRequestAsync`, baris 1023-1034). Silver dan Gold boleh aktif bersamaan, yang dibedakan hanya `IsPrimary` | Pasien bisa punya Silver dan Gold aktif sekaligus. Status yang tampil tergantung mana yang `IsPrimary` |
| `CRM-CONF-006` | `CRM-DEC-005`: perpanjangan membuat periode baru (bisa pada tingkat yang sama) | Larangan "Pasien sudah memiliki membership tier tersebut" berlaku juga untuk membership lama yang sudah tidak aktif atau kedaluwarsa (hanya `IsDelete` yang dikecualikan) | Pasien Gold yang memperpanjang ke Gold periode berikutnya ditolak dengan kode 400 |
| `CRM-CONF-007` | `CRM-DEC-011`: hanya CRM/Marketing yang boleh mendaftar, upgrade, membatalkan | (a) `POST /` dan `POST /kiosk` dibuka untuk akun perangkat kiosk. Kiosk bisa membuat membership aktif tanpa izin `PatientMembership : Create`. (b) Frontend memanggil `GET /` dan `POST /` (policy `KioskRead`), bukan rute `/admin`. Petugas Marketing tanpa role Administrator akan ditolak (kode 403), meskipun izin `PatientMembership` sudah diberikan lewat Role Access | (a) Perangkat kiosk menjadi celah pemberian diskon. (b) Petugas CRM tidak bisa memakai halaman yang sudah ada |
| `CRM-CONF-008` | `CRM-DEC-013`: pembatalan wajib alasan dan riwayat utuh | `PATCH /{id}/status` hanya membalik `IsActive` tanpa alasan. `PUT /{id}` boleh mengganti `PatientId`, tingkat, dan tanggal pada data lama (riwayat tertimpa). `DELETE` menghapus lunak dengan alasan opsional | Membership Gold milik Ibu Sari bisa "dipindah" ke pasien lain lewat ubah data, tanpa jejak periode lama |

## Kontrak As-Is

### Health Services / Patient Management / Master Data / Patient Membership

Base URL: `api/v1/health-services/patient-management/master-data/patient-memberships`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/filters/metadata`, `/kiosk/filters/metadata` | Pilihan filter dan enum | Policy `KioskRead` | - | `PatientMembershipFilterMetadataResponse` |
| `GET` | `/admin/filters/metadata` | Sama, untuk petugas | `PatientMembership : Read` | - | sama |
| `GET` | `/summary`, `/admin/summary` | Jumlah membership per kategori | `PatientMembership : Read` | - | `PatientMembershipSummaryResponse` |
| `GET` | `/`, `/kiosk` | Daftar membership | Policy `KioskRead` | query filter/paging | `PagedResult<PatientMembershipResponse>` |
| `GET` | `/admin` | Daftar membership untuk petugas | `PatientMembership : Read` | query filter/paging | sama |
| `GET` | `/options`, `/kiosk/options`, `/admin/options` | Pilihan dropdown | `KioskRead` / `PatientMembership : Read` | query | daftar `PatientMembershipOptionResponse` |
| `GET` | `/{id}`, `/admin/{id}` | Detail satu membership | `PatientMembership : Read` | - | `PatientMembershipDetailResponse` |
| `POST` | `/`, `/kiosk` | Membuat membership | Policy `KioskRead` (**tanpa** izin Create) | `CreatePatientMembershipRequest` | `PatientMembershipCreateResponse` |
| `POST` | `/admin` | Membuat membership | `PatientMembership : Create` | `CreatePatientMembershipRequest` | sama |
| `PUT` | `/{id}`, `/admin/{id}` | Mengubah seluruh isi membership | `PatientMembership : Update` | `UpdatePatientMembershipRequest` | `object` |
| `PATCH` | `/{id}/status`, `/admin/{id}/status` | Mengaktifkan/menonaktifkan | `PatientMembership : Update` | `UpdatePatientMembershipStatusRequest` (`IsActive`) | `object` |
| `DELETE` | `/{id}`, `/admin/{id}` | Hapus lunak | `PatientMembership : Delete` | `DeletePatientMembershipRequest` (alasan opsional) | `object` |

Kode status yang muncul:

- **400:** isian tidak valid. Contoh: "Nomor member sudah digunakan." atau "Pasien sudah memiliki membership tier tersebut."
- **403:** pengguna tidak punya izin, atau bukan akun kiosk/administrator pada rute ber-policy `KioskRead`.
- **404:** membership tidak ditemukan atau sudah dihapus.
- **500:** kesalahan server saat menyimpan.

### Administrator / Master Data / Membership Tier

Base URL: `api/v1/administrator/master-data/membership-tiers`. Menyediakan metadata filter,
ringkasan, daftar, pilihan, detail, tambah, ubah, dan ubah status tingkat
(`MembershipTierController.cs:64-604`).

> Catatan frontend: slice membership mencoba `/v1/health-services/master-data/membership-tiers`
> lebih dulu, lalu jatuh ke rute administrator (`master-data-patient-membership-slice.jsx:6-7`).
> Rute pertama tidak ditemukan di backend, sehingga setiap pemanggilan diawali satu permintaan
> gagal. Tidak memblokir.

## Jawaban atas Blocker Desain

| Open question | Jawaban berbasis bukti | Dampak |
|---|---|---|
| `CRM-OQ-005`: apakah kasir bisa menagih iuran yang tidak terikat kunjungan? | **Tidak.** `BilInvoice.EncounterId` wajib, dan piutang non-pasien hanya untuk parkir/tenant. Status `CRM-CAP-008` = `Missing` | Aktivasi setelah lunas (`CRM-DEC-004`) butuh amendment `billing-kasir` untuk tagihan non-kunjungan. Pilihan bentuknya diputuskan di desain bersama owner billing |
| `CRM-OQ-006`: apakah data membership per pasien sudah ada? | **Sudah**, di BE (`MstPatientMembership`) dan FE (halaman patient-memberships), tetapi aturannya bertentangan dengan 6 keputusan (`CRM-CONF-003` s.d. `CRM-CONF-008`) | Arah yang wajar: memperluas dan memperbaiki milik PatientManagement, bukan membuat tabel baru. Keputusan akhir ada di tahap desain |

## Pertanyaan Penutup untuk Owner

1. **Pemilik tabel membership.** `MstPatientMembership` milik PatientManagement. Apakah CRM
   memperluas tabel itu (tanpa registrasi prefix baru), atau CRM menjadi modul tersendiri dengan
   prefix baru yang harus didaftarkan lebih dulu? (`CRM-CAP-013`)
2. **Rute kiosk untuk membuat membership** (`POST /kiosk`). Apakah kiosk memang pernah boleh
   mendaftarkan membership? Bila tidak, rute itu harus ditutup agar `CRM-DEC-011` terpenuhi.
   Keputusan ini menyentuh modul kiosk.
3. **Data lama.** Membership yang sudah tersimpan (bila ada) berstatus Aktif tanpa bukti bayar.
   Apakah diakui apa adanya, atau diverifikasi ulang? Perlu pengecekan DB dengan izin terpisah
   (`CRM-CAP-015`).
4. **Owner billing untuk tagihan iuran.** Siapa di `billing-kasir` yang menyetujui tagihan
   non-kunjungan? (`CRM-CAP-008`)

## Pemicu Impact Scan

Peta ini dianggap basi dan perlu impact scan terbatas bila salah satu berikut berubah setelah
`36422a4d` (BE) / `9aba9929d` (FE):

- `Areas/HealthServices/PatientManagement/MasterData/**/PatientMembership*`, `MstPatient.cs`, `PatientController.cs`;
- `Areas/Administrator/MasterData/**/MembershipTier*`, `Enums/Membership*`;
- `Areas/HealthServices/BillingManagement/Billing/Models/BilInvoice.cs`, `BillingDiscountService.cs`, `BillingSettlementService.cs`;
- `Program.cs` (policy `KioskRead`), `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`;
- FE `src/app/health-services/patient-management/master-data/patient-memberships/**`, slice membership, `src/utils/menu-sidebar/menu-items.jsx`.
