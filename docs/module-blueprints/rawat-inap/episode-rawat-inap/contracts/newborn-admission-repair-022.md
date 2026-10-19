# Kontrak Perbaikan Admisi Bayi — `RWI-BAYI-022-2.0.0`

```yaml
contract_id: RWI-BAYI-022
version: 2.0.0
status: APPROVED
approved_by: Muhammad Hamzah (pengguna sesi)
approved_at: 2026-10-16
supersedes_for_new_flow: RWI-BAYI-016-1.0
scope: admisi_bayi_baru_lahir
excluded: perubahan_perilaku_batalkan_pendaftaran
```

Kontrak ini mengubah alur baru Admisi Bayi menjadi tujuh langkah. Data historis dari kontrak
`RWI-BAYI-016-1.0` tetap dapat dibaca, tetapi field preferensi invoice dan rencana deposit tidak
lagi ditulis oleh alur baru.

## 1. Invarian bisnis

1. Calon ibu berasal dari kasus operasi dengan `SurgicalServiceType = Obstetric` dan status
   `InProgress` atau `Completed`. Ruangan ibu tidak harus ruang operasi.
2. Setiap bayi mempunyai pasien, nomor rekam medis, encounter, episode, invoice, dan bed sendiri.
3. Nama sementara pasien adalah `By. Ny. <nama ibu>`.
4. Urutan kelahiran dialokasikan backend per `DeliveryCaseId`; browser tidak mengirim urutan.
5. Request berulang dengan `Idempotency-Key` yang sama dan isi sama mengembalikan pasien yang
   sama. Isi berbeda dengan kunci sama ditolak `409`.
6. Penjamin encounter bayi adalah snapshot turunan sumber pembayaran encounter ibu. FK kartu
   pasien milik ibu tidak dipasang sebagai kartu milik bayi.
7. Admisi tidak meminta nominal deposit atau pilihan penggabungan invoice. Invoice bayi tetap
   terpisah; penggunaan deposit ibu merupakan kewenangan Billing/Kasir/AR.
8. Hanya bed bayi pada unit rawat inap yang aktif dan tersedia untuk registrasi yang dapat dipilih.
9. Ringkasan lengkap ibu dan bayi hanya tampil pada langkah konfirmasi.
10. Perilaku tombol pembatalan yang sudah ada tidak diubah.

## 2. Alur tujuh langkah

| Urutan | Slug | Hasil |
| ---: | --- | --- |
| 1 | `newborn-context` | Kasus persalinan dan ibu terpilih |
| 2 | `newborn-identity` | Pasien bayi dan urutan lahir tersimpan |
| 3 | `doctor` | Rencana perawatan dan DPJP terverifikasi |
| 4 | `bed-selection` | Encounter, episode Draft, dan reservasi bed terbentuk |
| 5 | `consent-form` | Persetujuan orang tua/wali tersimpan |
| 6 | `confirmation` | Data final ditinjau dan admisi diaktifkan |
| 7 | `newborn-documents` | Gelang, label, dan dokumen bayi tersedia |

URL lama `step=payment` dan `step=deposit` pada mode bayi dialihkan ke `bed-selection`. Alur
pasien umum, pasien lama biasa, Kamar Pulih, dan transfer tidak berubah.

## 3. Kontrak endpoint

#### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/episodes/newborn-mother-candidates` | Daftar kasus persalinan yang dapat menjadi sumber ibu bayi | `InpatientEpisode : Read` |
| `POST` | `/api/v1/health-services/inpatient-management/episodes` | Membuat episode bayi tanpa input invoice/deposit | `InpatientEpisode : Create` |
| `GET` | `/api/v1/health-services/inpatient-management/episodes/{id}` | Membaca relasi ibu, kasus persalinan, dan konteks bayi | `InpatientEpisode : Read` |

`GET newborn-mother-candidates` menerima `search`, `fromDate`, `toDate`, `pageNumber`, dan
`pageSize`. Respons berhalaman memuat `deliveryCaseId`, nomor/status/waktu kasus, identitas ibu,
encounter sumber penjamin, payment source beserta versi waktunya, episode/lokasi ibu saat ini,
jumlah bayi terdaftar, dan preview urutan berikutnya.

#### Health Services / Patient Management / Master Data / Patient

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/patient-management/master-data/patients/newborns` | Membuat pasien bayi dan mengalokasikan urutan per kasus secara atomik | `Patient : Create` |

Header `Idempotency-Key` wajib. Body tidak menerima `BirthOrder`; field wajib adalah
`MotherPatientId`, `DeliveryCaseId`, `BirthDate`, `BirthTime`, dan `Gender`. Respons memuat
identitas pasien, nomor rekam medis, `DeliveryCaseId`, dan `BirthOrder` final.

#### Health Services / Registration Management / Patient Encounter

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/registration-management/patient-encounters/admin` | Membuat encounter bayi dengan sumber pembayaran turunan | `PatientEncounter : Create` |

Untuk alur bayi, request mengirim `InheritedPaymentSourceId` dan
`InheritedPaymentSourceExpectedAt`. Field kartu pasien bayi tidak dikirim. Backend memvalidasi
relasi bayi–ibu–kasus–encounter sumber dan menyalin snapshot penjamin. Perubahan sumber setelah
kandidat dimuat ditolak `409` agar petugas memuat ulang.

## 4. Persistence

- `MstPatient` ditambah logical reference `DeliveryCaseId`, hash idempotency, dan fingerprint
  request. Index unik menjaga `(DeliveryCaseId, BirthOrder)` dan hash idempotency.
- `PatNewbornBirthSequence` menyimpan pencacah urutan terakhir per `DeliveryCaseId`. Alokasi
  memakai transaksi dan database advisory lock; tidak memakai `Count+1` atau `Max+1`.
- `RegPatientEncounterGuarantor` menyimpan `InheritedFromPaymentSourceId` dan waktu snapshot
  sumber. `PatientInsuranceId` serta `PatientCompanyGuarantorId` tetap `null` pada mode turunan.
- `InpEpisode.DeliveryCaseId` menyimpan logical reference kasus persalinan. Field
  `FamilyBillingPreference` dan `PlannedDepositAmount` dipertahankan hanya untuk pembacaan data
  historis.

Migration dibuat sebagai artefak source tetapi tidak diterapkan ke database oleh pekerjaan ini.

## 5. Keputusan UI

| Elemen | Kandidat | Status | Keputusan |
| --- | --- | --- | --- |
| Stepper dan aksi | Stepper existing, `BaseButton` | `REUSE` | Gunakan tujuh item tanpa mengubah perilaku tombol pembatalan |
| Pencarian ibu | `ResourceFilterSelect` | `REUSE` | Sumber data diganti ke candidate endpoint |
| Form identitas | `BaseDateField`, `BaseTimeField`, `BaseSelectField`, `BaseInputField` | `COMPOSE` | Nama/urutan baca-saja; data lahir memakai base field |
| Ringkasan final | `BaseDetailCard`, `InformationAlert` | `COMPOSE` | Hanya pada konfirmasi |
| Penempatan | Komponen bed selection/booking existing | `REUSE` | Tetap memakai kontrak versi bed dan pemulihan operasi |

`UI GATE: 5 kelompok — REUSE 3, COMPOSE 2, EXTEND 0, WRAP 0, NEW 0.`

## 6. Kode status penting

| Kode | Kondisi |
| ---: | --- |
| `400` | Bentuk request atau tanggal/jam lahir tidak valid |
| `404` | Ibu, kasus persalinan, pasien, atau sumber pembayaran tidak ditemukan |
| `409` | Idempotency conflict, sumber pembayaran berubah, atau konflik concurrency |
| `422` | Kasus bukan obstetri/status tidak layak, relasi ibu salah, atau konteks bayi tidak konsisten |

