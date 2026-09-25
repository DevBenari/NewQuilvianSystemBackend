# Bank Darah — Integration Contract

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` · Contract version `v2` — **`approved`** (isinya tidak bergerak sejak `v2`; ikut disetujui sebagai bagian set `v4`) |
| `last_changed_in` | `v2` |
| Owner | Pemilik arsitektur backend · pemilik BillingManagement (batas biaya) |
| `approved_by` / `approved_at` | `Sukmagp` / `2026-09-03` |
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
| Kunjungan & konteksnya | RegistrationManagement — `RegPatientEncounter` | Bank Darah | Baca | Sinkron | Rujukan `EncounterId` | — |
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

## 3. Batas Billing — **disetujui `DEC-BD-016`** (17 September 2026)

**Delta 17 September 2026.** `DEC-BD-016` disetujui `Sukmagp`, sehingga bagian ini berpindah dari "belum
dapat dirancang" menjadi kontrak yang berlaku. Implementasinya `BE-BD-013`
([laporan](../task/report/backend/BE-BD-013.md)). Nol batas integrasi lain berubah.

| Batas | Produsen | Konsumen | Arah | Keadaan |
| --- | --- | --- | --- | --- |
| Fakta biaya tindakan Bank Darah | Bank Darah | BillingManagement | Keluar | **Berlaku** — lewat `ClinicalMilestoneFactProducer` → `BillingFolioService.RecognizeMilestoneAsync`, sinkron sesudah perubahan klinis tersimpan |

### 3.1 Bentuk fakta

| Unsur | Nilai | Sumber |
| --- | --- | --- |
| `SourceContext` | `BloodBank` — konstanta server `BillingSourceContract.BloodBankSourceContext` | Tidak pernah dari client |
| `EffectType` | `BloodBankCharge` — konstanta server `BillingSourceContract.BloodBankChargeEffectType` | Tidak pernah dari client |
| Pemicu | `BbkBloodBankProcedure` berpindah `Recorded` → `Completed` | `POST /blood-bank-procedures/{id}/complete` |
| Satuan | **Tepat satu fakta per tindakan selesai**, berapa pun kantong yang diberikan | `DEC-BD-021`, `DEC-BD-016` |
| `SourceAggregateId` | `BbkBloodBankProcedure.BloodOrderId` — order darah | Kolom tersimpan |
| `SourceItemId` | `BbkBloodBankProcedure.Id` — tindakan | Kolom tersimpan |
| `EncounterId` | `BbkBloodOrder.EncounterId` dari order tindakan | Kolom tersimpan |
| `OccurredAt` | `OccurredAt` baris `BbkTransitionHistory` aksi `Complete` — waktu penyelesaian yang tersimpan | Bukan waktu kiriman |
| `Quantity` / `Unit` | `1` / `Tindakan` — menyebut tindakan, **bukan** kantong | Tetap |
| `TariffSnapshot` | `source`, `procedureRefId`, `procedureCode`, `procedureName`, `tariffId`, `patientClassId`, `serviceUnitId`, `unitPrice` | Kolom salinan `BE-BD-012`; `MstTariff` **tidak** dibaca ulang |
| `RuleSnapshot` | `milestone = BloodBankProcedureCompleted`, `procedureNumber`, `chargeBasis = PerProcedure` | Kolom tersimpan |
| `CorrelationId` | `BloodOrderId` | Kolom tersimpan |
| `CausationId` | Tidak diisi — mengikuti pola producer Laboratorium dan Radiologi | — |

Identitas fakta **tidak pernah** memakai `BloodUnitId`, `AllocationId`, id pemberian, maupun `PatientId`.
Karena itu dua kantong dalam satu tindakan tidak dapat melahirkan dua charge.

### 3.2 Aturan yang mengikat

- **Urutan.** Perpindahan status dan riwayat `Complete` disimpan lebih dulu; baru fakta diserahkan. Producer
  tidak pernah dipanggil selagi transaksi klinis masih terbuka, dan tidak ada transaksi terdistribusi
  Bank Darah–Billing.
- **Kegagalan Billing tidak membatalkan klinis.** Hasil `RejectedByBilling`, `OutcomeUnknown`,
  `ReconciliationRequired`, maupun galat tak terduga **tidak** mengembalikan tindakan ke `Recorded`.
  Hasilnya dilaporkan pada `BillingHandoff` jawaban aksi.
- **Status penyerahan tinggal di ledger `CliClinicalMilestoneFact`.** `BbkBloodBankProcedure` **tidak**
  mendapat kolom penagihan apa pun.
- **Idempotency.** Fakta disusun ulang seluruhnya dari data tersimpan, sehingga kiriman ulang identik.
  Producer mengembalikan `Replayed` untuk fakta yang sudah diterima; Billing tidak membuat charge kedua.
  Jalur kirim ulang: `POST /blood-bank-procedures/{id}/resend-cost-fact` (lihat `api-contract.md`).
- Bank Darah **tidak** menghitung tagihan; nominal pada fakta adalah rujukan salinan tarif, akibat
  finansialnya milik Billing (`BD-CAP-015`).
- **Koreksi pemberian tidak membalik biaya secara otomatis** (`DEC-BD-034`, `INV-BD-024`). Jalur koreksi
  tidak memanggil producer — tidak ada fakta pembatalan, refund, void, maupun penyesuaian. Kasus tepi
  `ARCH-BD-GAP-09` tertutup di sisi Bank Darah; kebijakan peninjauan finansialnya milik Billing.

**Riwayat — keadaan sampai 17 September 2026:** batas ini berstatus "tertahan `DEC-BD-016`, belum dapat
dirancang". Yang sudah pasti saat itu: biaya dari tindakan (`DEC-BD-021`), Bank Darah tidak menghitung tarif,
dan koreksi tidak membalik biaya. Penambahan konteks sumber pada `BillingSourceContract` belum disetujui,
sehingga penyaluran MUST NOT dibekukan menjadi kontrak, dan pola idempotency yang direncanakan mengikuti
`LabSpecimenService` — pola yang kini dipakai.

---

## 4. Integrasi yang sengaja **di luar** kontrak ini

| Sistem | Keadaan | Sebab |
| --- | --- | --- |
| PMI (penyediaan darah) | **Tidak ada sambungan teknis** | Seluruh pertukaran lewat manusia & dokumen fisik pada MVP (`DEC-BD-002`). Yang dirancang hanya pencatatan sisi MMC. Tidak ada mekanisme rekonsiliasi otomatis; pencocokan dilakukan manusia |
| HCLAB | **Tidak ada sambungan pada MVP** | Bukti hanya workstation `BANK DARAH`, kode `BBW`, Lab Sec `GL`; tidak ada kontrak/protokol/pemetaan (`DEC-BD-022`, `BD-CAP-024`). Tetap dicatat sebagai temuan penelusuran |
| Laboratorium (pemeriksaan umum) | **Berjalan sendiri-sendiri** | Pemeriksaan golongan darah & sampelnya milik Bank Darah (`DEC-BD-015`, `DEC-BD-018`). Bila kelak Laboratorium mengambil alih, wajib ada keputusan kepemilikan & aturan prioritas (`INV-BD-015`) — bukan sekarang |
| Mesin crossmatch / kesesuaian klinis | Di luar scope | Quilvian tidak menghitung kompatibilitas (`INV-BD-013`); hanya mencatat titik pemeriksaan |
