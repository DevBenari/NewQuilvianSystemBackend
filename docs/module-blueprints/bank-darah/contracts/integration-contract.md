# Bank Darah — Integration Contract

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` · Contract version `v2` — `draft` |
| `last_changed_in` | `v2` |
| Owner | Pemilik arsitektur backend · pemilik BillingManagement (batas biaya) |
| `approved_by` / `approved_at` | Kosong — `draft` |
| Sumber | `03-domain-architecture.md` revisi 6 §J · `02-existing-capability-map.md` revisi 2 |

Dokumen ini menetapkan batas baca/tulis Bank Darah terhadap modul lain. Bank Darah **hanya membaca**
data master/kunjungan hulu dan **tidak pernah mengubahnya**. Satu-satunya penulisan lintas modul adalah
titipan satu kolom kewenangan pada `MstServiceUnit`, yang dikerjakan pemilik Master Data.

**Perubahan pada `v2`: nol batas integrasi baru.** `DEC-BD-035` sampai `DEC-BD-038` seluruhnya berjalan
di dalam Bank Darah — masternya milik BDRS, penempatannya milik kantong, dan tidak ada pihak lain yang
menghasilkan maupun memakai datanya. Satu-satunya penambahan pada dokumen ini adalah bagian 1b di bawah:
sebuah batas yang **sengaja tidak disambungkan**, dicatat supaya tidak tersambung tanpa sengaja di
kemudian hari.

---

## 1b. Batas yang sengaja TIDAK disambungkan — `MstDrugStorageLocation`

| Batas | Pemilik | Dibaca Bank Darah | Ditulis Bank Darah | Keputusan |
| --- | --- | :---: | :---: | --- |
| `MstDrugStorageLocation` (cold storage farmasi) | HealthServices Master Data, berorientasi Farmasi | **Tidak** | **Tidak** | `DEC-BD-035` |

Master ini sudah ada di `Areas/HealthServices/MasterData/Models/MstDrugStorageLocation.cs`, punya tipe
`ColdStorage` beserta rentang suhu dan kode rak/shelf/bin, sehingga tampak seperti kandidat pakai-ulang
yang wajar. Ia **ditolak** `DEC-BD-035`, dan penolakan itu dicatat di sini — bukan hanya di arsitektur —
supaya tidak ada yang menyambungkannya belakangan dengan niat baik.

Alasan penolakan, terbaca dari isi master itu sendiri: `IsPharmacyLocation` (bawaan `true`),
`IsControlledDrugStorage`, `IsHighAlertStorage`, `IsAllowDispensing`, `IsAllowReceiving` — seluruhnya
aturan bisnis farmasi yang tidak berlaku bagi kantong darah. Kantong darah bukan obat, tidak
di-*dispensing*, dan tidak tunduk pada aturan narkotika. Memakainya ulang berarti menaruh dua pemilik
proses di atas satu tabel yang sama.

Arahnya juga bukan sebaliknya: Bank Darah **tidak** memperluas master farmasi dengan atribut darah.
Yang dibuat adalah master sendiri, `MstBloodStorageLocation`, yang bersih dari atribut farmasi.

Penggabungan menjadi satu `MstStorageLocation` bersama **ditunda**, bukan ditolak selamanya —
`DEC-BD-035` menempatkannya sebagai bahan evaluasi POST-MVP. Bila kelak digabungkan, yang harus
diselesaikan lebih dulu adalah **siapa pemilik master gabungannya**, bukan bentuk tabelnya.

---

## 1. Dependency internal (baca saja)

| Batas | Produsen (sumber kebenaran) | Konsumen | Arah | Sifat | Idempotency | Bila gagal / rekonsiliasi |
| --- | --- | --- | --- | --- | --- | --- |
| Identitas pasien | PatientManagement — `MstPatient` | Bank Darah | Baca | Sinkron, saat dibutuhkan | Rujukan `PatientId`; tak menyalin | Pasien tak ditemukan → order ditolak `404`; tak ada rekonsiliasi |
| Kunjungan & konteksnya | RegistrationManagement — `TrxPatientEncounter` | Bank Darah | Baca | Sinkron | Rujukan `EncounterId` | — |
| **Sinyal kunjungan berakhir** | RegistrationManagement (`EncounterStatus`) + InPatientManagement (`InpEpisode.PhysicallyLeftAt`) | `BbkEncounterStatusReader` | Baca | Sinkron/terjadwal | Dibaca per kebutuhan; adapter tak menyimpan keadaan sendiri | Bila status tak terbaca, order **tidak** boleh otomatis dianggap aktif maupun berakhir; keadaannya dilaporkan apa adanya (`ASM-BD-005`) |
| Dokter | HR — `MstDoctor` | Bank Darah | Baca | Sinkron | Rujukan `DoctorId` | — |
| Unit, klinik, ruangan, kelas | HealthServices Master Data | Bank Darah | Baca | Sinkron | Rujukan `Id` | — |
| Tindakan bertarif & tarif | Master Data / Billing | `BbkBloodBankProcedure` | Baca | Sinkron | **Snapshot** kode/nama/tarif disalin saat tindakan dicatat (pola `BD-CAP-008`) agar tak berubah retroaktif | — |
| Golongan darah (nilai) | Platform — enum `BloodType` | Bank Darah | Pakai | — | Dipakai apa adanya | — |

**Adapter status kunjungan (`BD-DOM-16`) — dua penyesuai (`DEC-BD-014`):**

| Jenis kunjungan | Sinyal "berakhir" | Sumber |
| --- | --- | --- |
| Rawat jalan & IGD | `EncounterStatus` ∈ {`Completed`, `Cancelled`, `NoShow`} | `EncounterStatus.cs` |
| Rawat inap | `InpEpisode.PhysicallyLeftAt` terisi (**bukan** `ClosedAt`) | `InpEpisode.cs` |

Bank Darah tidak mengubah status kunjungan atau episode. Bila petugas rawat inap lupa mengisi
`PhysicallyLeftAt`, order rawat inap tidak kedaluwarsa — ditangani sebagai kualitas data di modul
Inpatient (`ASM-BD-005`), bukan aturan cadangan di Bank Darah.

---

## 2. Titipan tulis ke Master Data (satu-satunya)

| Batas | Pemilik | Yang dititipkan | Catatan |
| --- | --- | --- | --- |
| Kewenangan unit memesan darah | HealthServices — Master Data | Kolom `IsAvailableForBloodOrder` (bool, default `false`) pada `MstServiceUnit` (`BD-DOM-18`) | Bergaya `IsAvailableFor*` yang sudah ada (`BD-CAP-005`). Bawaan menolak (`DEC-BD-012`). Pengelolaannya lewat kontrak unit pelayanan milik Master Data, bukan endpoint Bank Darah |

---

## 3. Batas Billing — **tertahan `DEC-BD-016`**

| Batas | Produsen | Konsumen | Arah | Keadaan |
| --- | --- | --- | --- | --- |
| Fakta biaya tindakan Bank Darah | Bank Darah | BillingManagement | Keluar | **Belum dapat dirancang** |

Yang sudah pasti dan dipatuhi:

- Biaya berasal dari **tindakan** (`DEC-BD-021`), bukan dari kantong. Beberapa kantong dalam satu
  tindakan tidak menghasilkan beberapa tagihan.
- Bank Darah **tidak** menghitung tarif; Billing pemilik akibat finansial (`BD-CAP-015`).
- **Koreksi pemberian tidak membalik biaya secara otomatis** (`DEC-BD-034`, `INV-BD-024`). Keputusan
  peninjauan biaya milik Billing.

Yang belum boleh dibekukan:

- Penambahan konteks sumber Bank Darah pada `BillingSourceContract` belum disetujui pemilik Billing
  (`DEC-BD-016`). Sampai turun, kejadian "tindakan selesai" boleh dirancang sebagai kejadian domain,
  tetapi **penyalurannya ke Billing MUST NOT dibekukan menjadi kontrak**.
- Kasus tepi `ARCH-BD-GAP-09` (koreksi menghapus satu-satunya pemberian di bawah tindakan) tetap Open
  Question milik Billing, menempel `DEC-BD-016`.

Pola idempotency yang **akan** dipakai bila kontrak turun: snapshot tarif per baris + pengenalan
kiriman ulang sebagai kiriman ulang (bukan tagihan baru), mengikuti `LabSpecimenService` (`BD-CAP-015`).

---

## 4. Integrasi yang sengaja **di luar** kontrak ini

| Sistem | Keadaan | Sebab |
| --- | --- | --- |
| PMI (penyediaan darah) | **Tidak ada sambungan teknis** | Seluruh pertukaran lewat manusia & dokumen fisik pada MVP (`DEC-BD-002`). Yang dirancang hanya pencatatan sisi MMC. Tidak ada mekanisme rekonsiliasi otomatis; pencocokan dilakukan manusia |
| HCLAB | **Tidak ada sambungan pada MVP** | Bukti hanya workstation `BANK DARAH`, kode `BBW`, Lab Sec `GL`; tidak ada kontrak/protokol/pemetaan (`DEC-BD-022`, `BD-CAP-024`). Tetap dicatat sebagai temuan penelusuran |
| Laboratorium (pemeriksaan umum) | **Berjalan sendiri-sendiri** | Pemeriksaan golongan darah & sampelnya milik Bank Darah (`DEC-BD-015`, `DEC-BD-018`). Bila kelak Laboratorium mengambil alih, wajib ada keputusan kepemilikan & aturan prioritas (`INV-BD-015`) — bukan sekarang |
| Mesin crossmatch / kesesuaian klinis | Di luar scope | Quilvian tidak menghitung kompatibilitas (`INV-BD-013`); hanya mencatat titik pemeriksaan |
