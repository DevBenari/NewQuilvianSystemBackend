# State Transition Matrix — Modul IGD

| Field | Nilai |
| --- | --- |
| Blueprint | `IGD-BP-001` revision `4` |
| `contract_version` | `0.2.0-draft` |
| Commit diaudit | backend `e5331a0` |

Transisi yang **tidak** sah ikut dituliskan, bukan hanya yang sah. Tanpa itu, implementer
menebak apa yang harus ditolak.

---

## 1. Status kunjungan — `EmergencyVisitStatus`

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Pasien tiba | `Arrived` | Petugas pendaftaran atau perawat | Unit IGD terisi | — |
| `Arrived` | Masuk antrean triage | `WaitingForTriage` | Perawat | — | 409 |
| `WaitingForTriage` | Triage selesai | `Triaged` | Perawat | Ada `TrxEmergencyTriage` berstatus `Completed` | 409 |
| `Triaged` | Mulai penanganan | `InTreatment` | Dokter atau perawat | — | 409 |
| `InTreatment` | Mulai observasi | `UnderObservation` | Dokter atau perawat | Ada `TrxEmergencyObservation` aktif | 409 |
| `InTreatment` | Menunggu keputusan | `AwaitingDisposition` | Dokter | — | 409 |
| `UnderObservation` | Menunggu keputusan | `AwaitingDisposition` | Dokter | Observasi berstatus `Completed` atau `Escalated` | 409 |
| `AwaitingDisposition` | Keputusan ditetapkan | `Disposed` | Dokter | Ada `TrxEmergencyDisposition` berstatus `Confirmed` atau `Executed` | 409 |
| **`Disposed`** | **Selesaikan secara klinis** | **`Completed`** | **Dokter penanggung jawab** | **Seluruh closure gate klinis dan transfer terpenuhi; `VisitCompletedAt` terisi** | **409** |
| `Arrived` sampai `AwaitingDisposition` | Batalkan | `Cancelled` | Kepala jaga | Wajib mengisi alasan | 400 bila alasan kosong |

### Transisi yang tidak sah

| Transisi | Alasan penolakan |
| --- | --- |
| `Disposed` ke `InTreatment` | Keputusan akhir sudah ditetapkan; penanganan baru memerlukan kunjungan baru |
| `Completed` ke status mana pun | Penyelesaian klinis bersifat final; koreksi memakai jalur correction yang tercatat |
| `Cancelled` ke status mana pun | Kunjungan yang dibatalkan tidak dibuka kembali |
| Melompati `Triaged` | Pasien tidak boleh ditangani sebelum dinilai, kecuali jalur `ImmediateCareAllowed` |

### Pengecualian keselamatan

Pasien dengan `IsImmediateCareAllowed` bernilai benar boleh masuk `InTreatment` sebelum
registrasi selesai. Ini jalur keselamatan dan **tidak boleh** diblokir oleh syarat
administratif apa pun.

Penutupan klinis **tidak** bergantung pada status billing, sesuai `IGD-DEC-021`. Billing yang
belum final tidak membuat pasien tetap dianggap aktif secara klinis.

---

## 2. Status triage — `EmergencyTriageStatus`

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Mulai penilaian | `Draft` | Perawat | Kunjungan aktif | — |
| `Draft` | Simpan penilaian | `InProgress` | Perawat | Ringkasan ABCDE terisi minimal satu | 400 |
| `InProgress` | Selesaikan | `Completed` | Perawat | `TriageLevelId` terisi; `ResponseDueAt` terhitung | 400 |
| `Completed` | Nilai ulang | `Superseded` | Perawat | Ada penilaian baru yang menunjuk baris ini lewat `PreviousTriageId` | 409 |
| `Draft` atau `InProgress` | Batalkan | `Cancelled` | Kepala jaga | Wajib mengisi alasan | 400 bila alasan kosong |

### Transisi yang tidak sah

| Transisi | Alasan penolakan |
| --- | --- |
| `Cancelled` ke `Superseded` | Penilaian yang dibatalkan tidak pernah berlaku, sehingga tidak dapat digantikan |
| `Superseded` ke status mana pun | Baris yang sudah digantikan bersifat historis |
| `Completed` ke `InProgress` | Koreksi memakai retriage, bukan pengubahan baris lama |

`Superseded` dipakai **hanya** ketika penilaian digantikan retriage, bukan saat dibatalkan.
Ini menutup `IGD-CONFLICT-001` versi lama pada decision log.

---

## 3. Status disposition — `EmergencyDispositionStatus`

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| — | Buat keputusan | `Draft` | Dokter | Kunjungan berstatus `AwaitingDisposition` |
| `Draft` | Konfirmasi | `Confirmed` | Dokter penanggung jawab | Unit tujuan atau fasilitas rujukan terisi bila jenis disposition mensyaratkannya |
| `Confirmed` | Laksanakan | `Executed` | Perawat | Transfer terkait sudah `Completed` bila disposition memerlukan perpindahan |
| `Draft` atau `Confirmed` | Batalkan | `Cancelled` | Dokter penanggung jawab | Wajib mengisi alasan |

`Executed` pada disposition tidak otomatis menyelesaikan kunjungan. Penyelesaian klinis tetap
melalui transisi `Disposed` ke `Completed` pada kunjungan.

---

## 4. Status transfer — `EmergencyTransferStatus`

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| — | Ajukan | `Requested` | Perawat IGD | Unit tujuan terisi |
| `Requested` | Terima | `Accepted` | Petugas unit tujuan | Kewenangan pada unit tujuan |
| `Requested` | Tolak | `Rejected` | Petugas unit tujuan | Wajib mengisi alasan |
| `Accepted` | Berangkat | `InTransit` | Perawat pengantar | — |
| `InTransit` | Tiba | `Completed` | Perawat penerima | Handover tercatat |
| `Requested` atau `Accepted` | Batalkan | `Cancelled` | Perawat IGD | Wajib mengisi alasan |

Kewenangan pengaju dan penerima **wajib dipisahkan**. Satu pengguna tidak boleh mengajukan
sekaligus menerima transfer yang sama.

---

## 5. Status observasi dan resusitasi

### `EmergencyObservationStatus`

| Dari | Tindakan | Ke | Syarat |
| --- | --- | --- | --- |
| — | Mulai observasi | `Active` | Kunjungan aktif |
| `Active` | Selesaikan | `Completed` | Kesimpulan terisi |
| `Active` | Eskalasi | `Escalated` | Alasan eskalasi terisi |
| `Active` | Batalkan | `Cancelled` | Alasan terisi |

### `EmergencyResuscitationStatus`

| Dari | Tindakan | Ke | Syarat |
| --- | --- | --- | --- |
| — | Rencanakan | `Planned` | Kunjungan aktif |
| `Planned` | Mulai | `InProgress` | Ketua tim terisi |
| `InProgress` | Selesaikan | `Completed` | Hasil akhir terisi |
| `InProgress` | Hentikan | `Stopped` | Alasan penghentian terisi |
| `Planned` atau `InProgress` | Batalkan | `Cancelled` | Alasan terisi |

---

## 6. Status registrasi — `EmergencyRegistrationStatus`

| Dari | Tindakan | Ke | Syarat |
| --- | --- | --- | --- |
| — | Pasien tiba | `Pending` | — |
| `Pending` | Buat encounter provisional | `Provisional` | Pasien gawat atau belum teridentifikasi |
| `Pending` atau `Provisional` | Lengkapi identitas | `Registered` | Identitas pasien terverifikasi |
| `Registered` | Selesaikan administrasi | `Completed` | Berkas administrasi lengkap |
| Mana pun kecuali `Completed` | Batalkan | `Cancelled` | Alasan terisi |

Status registrasi berjalan **paralel** dengan status kunjungan dan tidak memblokirnya.
Pasien dapat berada pada `InTreatment` sementara registrasi masih `Provisional`.
